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
            FreezableStepData freezableStepData, TypeResolver typeResolver) =>
            Maybe<ITypeReference>.None;

        /// <inheritdoc />
        public virtual IStepSerializer Serializer => new FunctionSerializer(TypeName);


        /// <inheritdoc />
        public virtual Maybe<IStepCombiner> StepCombiner => Maybe<IStepCombiner>.None;

        /// <inheritdoc />
        public virtual IEnumerable<Requirement> Requirements => ImmutableArray<Requirement>.Empty;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        protected abstract Result<ICompoundStep, IError> TryCreateInstance(StepContext stepContext, FreezableStepData freezableStepData);

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
        public Result<IStep, IError> TryFreeze(StepContext stepContext, FreezableStepData freezableStepData, Configuration? configuration)
        {
            var instanceResult = TryCreateInstance(stepContext, freezableStepData);

            if (instanceResult.IsFailure)
                return instanceResult.ConvertFailure<IStep>();

            var step = instanceResult.Value;
            step.Configuration = configuration;

            var results = new List<Result<Unit, IError>>();

            var instanceType = step.GetType();


            var simpleProperties1 = instanceType.GetProperties()
                .Where(x => x.GetCustomAttribute<StepPropertyAttribute>() != null)
                .ToDictionary(x=>x.Name!, StringComparer.OrdinalIgnoreCase);

            var variableNameProperties1 = instanceType
                .GetProperties()
                .Where(x => x.PropertyType == typeof(VariableName))
                .Where(x => x.GetCustomAttribute<VariableNameAttribute>() != null)
                .ToDictionary(x => x.Name!, StringComparer.OrdinalIgnoreCase);

            var listProperties1 = instanceType.GetProperties()
                .Where(x => x.GetCustomAttribute<StepListPropertyAttribute>() != null)
                .ToDictionary(x=>x.Name!, StringComparer.OrdinalIgnoreCase);



            var r1 = SetFromDictionary(freezableStepData.StepDictionary,
                (pi, f) => TrySetStep(pi, step, f, stepContext),
                simpleProperties1!,
                step,
                TypeName);

            var r2 = SetFromDictionary(freezableStepData.VariableNameDictionary,
                (pi, f) => TrySetVariableName(pi, step, f),
                variableNameProperties1!,
                step,
                TypeName);

            var r3 = SetFromDictionary(freezableStepData.StepListDictionary,
                (pi, f) => TrySetStepList(pi, step, f, stepContext),
                listProperties1!,
                step,
                TypeName);

            results.Add(r1);
            results.Add(r2);
            results.Add(r3);

            var remainingProperties = simpleProperties1
                .Concat(variableNameProperties1)
                .Concat(listProperties1)
                .Select(x=>x.Value)
                .Distinct();


            foreach (var property in remainingProperties
                .Where(property => property.GetCustomAttribute<RequiredAttribute>() != null))
            {
                var error = new SingleError($"The property '{property.Name}' was not set on type '{GetType().Name}'.", ErrorCode.MissingParameter, new StepErrorLocation(step));

                results.Add(error);
            }


            return results.Combine(ErrorList.Combine).Map(_ => step as IStep);

        }


        private static Result<Unit, IError> SetFromDictionary<T>(IReadOnlyDictionary<string, T> dict,
                Func<PropertyInfo, T, Result<Unit, IError>> trySet,
                IDictionary<string, PropertyInfo> remaining,
                ICompoundStep parentStep,
                string typeName)
        {
            var results = new List<Result<Unit, IError>>();


            foreach (var (propertyName, stepMember) in dict)
            {
                if (remaining.Remove(propertyName, out var propertyInfo))
                    results.Add(trySet(propertyInfo, stepMember));
                else
                    results.Add(Result.Failure<Unit, IError>(ErrorHelper.UnexpectedParameterError(propertyName, typeName).WithLocation(parentStep)));
            }

            return results.Combine(_=> Unit.Default, ErrorList.Combine);
        }

        private static Result<Unit, IError> TrySetVariableName(PropertyInfo propertyInfo, ICompoundStep parentStep, VariableName member)
        {
            propertyInfo.SetValue(parentStep, member);
            return Unit.Default;
        }

        private static Result<Unit, IError> TrySetStep(PropertyInfo propertyInfo, ICompoundStep parentStep, IFreezableStep freezableStep, StepContext context)
        {
            var argumentFreezeResult = freezableStep.TryFreeze(context);
            if (argumentFreezeResult.IsFailure)
                return argumentFreezeResult.ConvertFailure<Unit>();

            var frozenStep = argumentFreezeResult.Value;


            if (!propertyInfo.PropertyType.IsInstanceOfType(frozenStep))
                return new SingleError($"'{propertyInfo.Name}' cannot take the value '{frozenStep}'", ErrorCode.InvalidCast, new StepErrorLocation(parentStep));

            propertyInfo.SetValue(parentStep, frozenStep); //This could throw an exception but we don't expect it.
            return Unit.Default;
        }



        private static Result<Unit, IError> TrySetStepList(PropertyInfo propertyInfo, ICompoundStep parentStep, IReadOnlyList<IFreezableStep> member, StepContext context)
        {
            var freezeResult =
                member.Select(x => x.TryFreeze(context)).Combine(ErrorList.Combine)
                        .Map(x => x.ToImmutableArray());
            if (freezeResult.IsFailure)
                return freezeResult.ConvertFailure<Unit>();

            var genericType = propertyInfo.PropertyType.GenericTypeArguments.Single();
            var listType = typeof(List<>).MakeGenericType(genericType);

            var list = Activator.CreateInstance(listType);

            foreach (var step1 in freezeResult.Value)
                if (genericType.IsInstanceOfType(step1))
                {
                    var addMethod = listType.GetMethod(nameof(List<object>.Add))!;
                    addMethod.Invoke(list, new object?[] { step1 });
                }
                else
                    return new SingleError(
                        $"'{CompressSpaces(step1.Name)}' is a '{step1.OutputType.GetDisplayName()}' but it should be a '{genericType.GenericTypeArguments.First().GetDisplayName()}' to be a member of '{parentStep.StepFactory.TypeName}'",
                        ErrorCode.InvalidCast,
                        new StepErrorLocation(parentStep));


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