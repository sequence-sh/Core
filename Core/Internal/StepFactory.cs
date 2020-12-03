using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Namotion.Reflection;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A factory for creating steps.
    /// </summary>
    public abstract class StepFactory : IStepFactory
    {
        /// <inheritdoc />
        public abstract Result<ITypeReference, IError> TryGetOutputTypeReference(FreezableStepData freezableStepData, TypeResolver typeResolver);

        /// <inheritdoc />
        public string TypeName => FormatTypeName(StepType);

        /// <summary>
        /// The type of this step.
        /// </summary>
        public abstract Type StepType { get; }


        /// <inheritdoc />
        public override string ToString() => TypeName;

        /// <inheritdoc />
        public abstract IStepNameBuilder StepNameBuilder { get; }

        /// <inheritdoc />
        public virtual IEnumerable<(VariableName VariableName, ITypeReference typeReference)> FixedVariablesSet =>
            Enumerable.Empty<(VariableName VariableName, ITypeReference typeReference)>();

        /// <inheritdoc />
        public abstract IEnumerable<Type> EnumTypes { get; }

        /// <inheritdoc />
        public abstract string OutputTypeExplanation { get; }


        /// <inheritdoc />
        public virtual Result<Maybe<ITypeReference>, IError> GetTypeReferencesSet(VariableName variableName,
            FreezableStepData freezableStepData, TypeResolver typeResolver, StepFactoryStore stepFactoryStore) =>
            Maybe<ITypeReference>.None;

        /// <inheritdoc />
        public virtual IStepSerializer Serializer => new FunctionSerializer(TypeName);

        /// <inheritdoc />
        public virtual IEnumerable<Requirement> Requirements => ImmutableArray<Requirement>.Empty;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        protected abstract Result<ICompoundStep, IError> TryCreateInstance(StepContext stepContext, FreezableStepData freezeData);

        /// <inheritdoc />
        public (MemberType memberType, Type? type) GetExpectedMemberType(string name)
        {
            var propertyInfo = StepType.GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null) return (MemberType.NotAMember, null);

            if (propertyInfo.GetCustomAttribute<VariableNameAttribute>() != null) return (MemberType.VariableName, null);
            if (propertyInfo.GetCustomAttribute<StepPropertyAttribute>() != null) return (MemberType.Step, propertyInfo.PropertyType.GenericTypeArguments.First());
            if (propertyInfo.GetCustomAttribute<StepListPropertyAttribute>() != null) return (MemberType.StepList, propertyInfo.PropertyType.GenericTypeArguments.First().GenericTypeArguments.First());

            return (MemberType.NotAMember, null);
        }

        /// <inheritdoc />
        public IEnumerable<string> RequiredProperties =>
            StepType.GetProperties()
                .Where(property => property.GetCustomAttribute<RequiredAttribute>() != null).Select(property => property.Name);

        /// <inheritdoc />
        public Result<IStep, IError> TryFreeze(StepContext stepContext,
            FreezableStepData freezeData, Configuration? configuration)
        {
            var instanceResult = TryCreateInstance(stepContext, freezeData);

            if (instanceResult.IsFailure)
                return instanceResult.ConvertFailure<IStep>();

            var step = instanceResult.Value;
            step.Configuration = configuration;

            var results = new List<Result<Unit, IError>>();

            var instanceType = step.GetType();

            var remaining = instanceType.GetProperties()
                    .Where(x => x.GetCustomAttribute<StepPropertyBaseAttribute>() != null)
                    .ToDictionary(x=>x.Name!, StringComparer.OrdinalIgnoreCase);

            foreach (var (propertyName, stepMember) in freezeData.StepProperties)
            {
                if (remaining.Remove(propertyName, out var propertyInfo))
                {
                    var result =
                    stepMember.Match(
                        vn => TrySetVariableName(propertyInfo, step, vn),
                        s => TrySetStep(propertyInfo, step, s, stepContext),
                        sList => TrySetStepList(propertyInfo, step, sList, stepContext)
                    );


                    results.Add(result);
                }
                else
                    results.Add(
                        Result.Failure<Unit, IError>(
                            ErrorHelper.UnexpectedParameterError(propertyName, TypeName)
                                .WithLocation(freezeData.Location)));
            }

            results.Combine(_ => Unit.Default, ErrorList.Combine);


            foreach (var property in remaining.Values
                .Where(property => property.GetCustomAttribute<RequiredAttribute>() != null))
            {
                var error = new SingleError($"The property '{property.Name}' was not set on type '{GetType().Name}'.", ErrorCode.MissingParameter, new StepErrorLocation(step));

                results.Add(error);
            }


            return results.Combine(ErrorList.Combine).Map(_ => step as IStep);

        }


        private static Result<Unit, IError> TrySetVariableName(PropertyInfo propertyInfo, ICompoundStep parentStep, VariableName member)
        {
            propertyInfo.SetValue(parentStep, member);
            return Unit.Default;
        }

        private static Result<Unit, IError> TrySetStep(PropertyInfo propertyInfo,
            ICompoundStep parentStep,
            IFreezableStep freezableStep,
            StepContext stepContext)
        {
            var freezeResult = freezableStep.TryFreeze(stepContext);

            if (freezeResult.IsFailure) return freezeResult.ConvertFailure<Unit>();

            if (!propertyInfo.PropertyType.IsInstanceOfType(freezeResult.Value))
                return new SingleError($"'{propertyInfo.Name}' cannot take the value '{freezeResult.Value}'", ErrorCode.InvalidCast, new StepErrorLocation(parentStep));

            propertyInfo.SetValue(parentStep, freezeResult.Value); //This could throw an exception but we don't expect it.
            return Unit.Default;
        }



        private static Result<Unit, IError> TrySetStepList(PropertyInfo propertyInfo,
            ICompoundStep parentStep,
            IReadOnlyList<IFreezableStep> freezableStepList,
            StepContext stepContext)
        {
            var argument = propertyInfo.PropertyType.GenericTypeArguments.Single();
            var listType = typeof(List<>).MakeGenericType(argument);

            var list = Activator.CreateInstance(listType);
            var errors = new List<IError>();


            foreach (var freezableStep in freezableStepList)
            {
                var freezeResult = freezableStep.TryFreeze(stepContext);

                if(freezeResult.IsFailure)
                    errors.Add(freezeResult.Error);
                else if (freezeResult.Value is IConstantStep constant && argument.IsInstanceOfType(constant.ValueObject))
                {
                    var addMethod = listType.GetMethod(nameof(List<object>.Add))!;
                    addMethod.Invoke(list, new[] { constant.ValueObject });
                }
                else if (argument.IsInstanceOfType(freezeResult.Value))
                {
                    var addMethod = listType.GetMethod(nameof(List<object>.Add))!;
                    addMethod.Invoke(list, new object?[] { freezeResult.Value });
                }
                else
                {
                    var error = new SingleError(
                        $"'{CompressSpaces(freezeResult.Value.Name)}' is a '{freezeResult.Value.OutputType.GetDisplayName()}' but it should be a '{argument.GenericTypeArguments.First().GetDisplayName()}' to be a member of '{parentStep.StepFactory.TypeName}'",
                        ErrorCode.InvalidCast,
                        new StepErrorLocation(parentStep));
                    errors.Add(error);
                }
            }

            if (errors.Any())
                return Result.Failure<Unit, IError>(ErrorList.Combine(errors));

            propertyInfo.SetValue(parentStep, list);

            return Unit.Default;
        }


        private static readonly Regex SpaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
        private static string CompressSpaces(string stepName) => SpaceRegex.Replace(stepName, " ");


        /// <summary>
        /// Creates a typed generic step with one type argument.
        /// </summary>
        protected static Result<ICompoundStep, IErrorBuilder> TryCreateGeneric(Type openGenericType, Type parameterType)
        {
            object? r;

            try
            {
                var genericType = openGenericType.MakeGenericType(parameterType);
                r = Activator.CreateInstance(genericType);
            }
            catch (ArgumentException e)
            {
                if (e.Message.Contains("violates the constraint of type") && openGenericType == typeof(Compare<>))
                {
                    var parameterTypeName = parameterType.GetDisplayName();
                    return new ErrorBuilder($"Cannot compare objects of type '{parameterTypeName}'", ErrorCode.InvalidCast);
                }

                return new ErrorBuilder(e, ErrorCode.InvalidCast);
            }

#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                return new ErrorBuilder(e, ErrorCode.InvalidCast);
            }
#pragma warning restore CA1031 // Do not catch general exception types


            if (r is ICompoundStep rp)
                return Result.Success<ICompoundStep, IErrorBuilder>(rp);

            return new ErrorBuilder(
                $"Could not create an instance of {openGenericType.Name.Split("`")[0]}<{parameterType.GetDisplayName()}>",
                ErrorCode.InvalidCast);
        }

        /// <summary>
        /// Gets the name of the type, removing the backtick if it is a generic type.
        /// </summary>
        protected static string FormatTypeName(Type type)
        {
            string friendlyName = type.Name;
            if (type.IsGenericType)
            {
                var iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0) friendlyName = friendlyName.Remove(iBacktick);
            }

            return friendlyName;
        }

        /// <inheritdoc />
        public virtual string Category
        {
            get
            {
                var fullName = StepType.Assembly.GetName().Name!;
                var lastTerm = GetLastTerm(fullName);

                return lastTerm;
            }
        }

        private static string GetLastTerm(string s) => s.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();
    }
}