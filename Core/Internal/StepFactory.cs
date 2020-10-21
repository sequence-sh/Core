using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Namotion.Reflection;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Steps;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A factory for creating steps.
    /// </summary>
    public abstract class StepFactory : IStepFactory
    {
        /// <inheritdoc />
        public abstract Result<ITypeReference> TryGetOutputTypeReference(FreezableStepData freezableStepData,
            TypeResolver typeResolver);

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
        public abstract IEnumerable<Type> EnumTypes { get; }

        /// <inheritdoc />
        public abstract string OutputTypeExplanation { get; }


        /// <inheritdoc />
        public virtual Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName,
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
        protected abstract Result<ICompoundStep> TryCreateInstance(StepContext stepContext, FreezableStepData freezableStepData);

        /// <inheritdoc />
        public MemberType GetExpectedMemberType(string name)
        {
            var propertyInfo = StepType.GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null) return MemberType.NotAMember;

            if (propertyInfo.GetCustomAttribute<VariableNameAttribute>() != null) return MemberType.VariableName;
            if (propertyInfo.GetCustomAttribute<StepPropertyAttribute>() != null) return MemberType.Step;
            if (propertyInfo.GetCustomAttribute<StepListPropertyAttribute>() != null) return MemberType.StepList;

            return MemberType.NotAMember;
        }


        /// <inheritdoc />
        public Result<IStep> TryFreeze(StepContext stepContext, FreezableStepData freezableStepData, Configuration? configuration)
        {
            var instanceResult = TryCreateInstance(stepContext, freezableStepData);

            if (instanceResult.IsFailure)
                return instanceResult.ConvertFailure<IStep>();

            var step = instanceResult.Value;
            step.Configuration = configuration;

            var errors = new List<Result>();

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
                MemberType.Step,
                TypeName);

            var r2 = SetFromDictionary(freezableStepData.VariableNameDictionary,
                (pi, f) => TrySetVariableName(pi, step, f),
                variableNameProperties1!,
                MemberType.VariableName,
                TypeName);

            var r3 = SetFromDictionary(freezableStepData.StepListDictionary,
                (pi, f) => TrySetStepList(pi, step, f, stepContext),
                listProperties1!,
                MemberType.StepList,
                TypeName);

            errors.AddRange(r1.Concat(r2).Concat(r3).Where(x=>x.IsFailure));

            var remainingProperties = simpleProperties1
                .Concat(variableNameProperties1)
                .Concat(listProperties1)
                .Select(x=>x.Value)
                .Distinct();

            errors.AddRange(remainingProperties
                .Where(property => property.GetCustomAttribute<RequiredAttribute>() != null)
                .Select(property => Result.Failure($"The property '{property.Name}' was not set on type '{GetType().Name}'."))
            );


            if (errors.Any())
                return errors.Combine("; ").ConvertFailure<IStep>();

            return Result.Success<IStep>(step);

        }


        private static IEnumerable<Result> SetFromDictionary<T>(IReadOnlyDictionary<string, T> dict,
                Func<PropertyInfo, T, Result> trySet,
                IDictionary<string, PropertyInfo> remaining,
                MemberType memberType,
                string typeName)
        {
            var results = new List<Result>();


            foreach (var (propertyName, stepMember) in dict)
            {
#pragma warning disable 8714
                if (remaining.Remove(propertyName, out var propertyInfo))
#pragma warning restore 8714
                {
                    results.Add(trySet(propertyInfo, stepMember));
                }
                else
                    results.Add(Result.Failure($"'{typeName}' does not have a property named '{propertyName}' of type '{memberType}'."));
            }

            return results;
        }

        private static Result TrySetVariableName(PropertyInfo propertyInfo, ICompoundStep parentStep, VariableName member)
        {
            propertyInfo.SetValue(parentStep, member);
            return Result.Success();
        }

        private static Result TrySetStep(PropertyInfo propertyInfo, ICompoundStep parentStep, IFreezableStep freezableStep, StepContext context)
        {
            var argumentFreezeResult = freezableStep.TryFreeze(context);
            if (argumentFreezeResult.IsFailure)
                return argumentFreezeResult;
            if (!propertyInfo.PropertyType.IsInstanceOfType(argumentFreezeResult.Value))
                return Result.Failure($"'{propertyInfo.Name}' cannot take the value '{argumentFreezeResult.Value}'");

            propertyInfo.SetValue(parentStep, argumentFreezeResult.Value); //This could throw an exception but we don't expect it.
            return Result.Success();
        }

        private static Result TrySetStepList(PropertyInfo propertyInfo, ICompoundStep parentStep, IReadOnlyList<IFreezableStep> member, StepContext context)
        {
            var freezeResult =
                member.Select(x => x.TryFreeze(context)).Combine()
                        .Map(x => x.ToImmutableArray());
            if (freezeResult.IsFailure)
                return freezeResult;

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
                    return Result.Failure($"'{step1.Name}' is a '{step1.OutputType.GetDisplayName()}' but it should be a '{genericType.GenericTypeArguments.First().GetDisplayName()}' to be a member of '{parentStep.StepFactory.TypeName}'");


            propertyInfo.SetValue(parentStep, list);

            return Result.Success();
        }


        /// <summary>
        /// Creates a typed generic step with one type argument.
        /// </summary>
        protected static Result<ICompoundStep> TryCreateGeneric(Type openGenericType, Type parameterType)
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
                    return Result.Failure<ICompoundStep>($"Cannot compare objects of type '{parameterTypeName}'");
                }

                return Result.Failure<ICompoundStep>(e.Message);
            }

#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                return Result.Failure<ICompoundStep>(e.Message);
            }
#pragma warning restore CA1031 // Do not catch general exception types


            if (r is ICompoundStep rp)
                return Result.Success(rp);

            return Result.Failure<ICompoundStep>($"Could not create an instance of {openGenericType.Name.Split("`")[0]}<{parameterType.GetDisplayName()}>");
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