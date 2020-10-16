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

        /// <summary>
        /// Gets all enum types used by this step.
        /// </summary>
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

        /// <summary>
        /// Gets the type of this member.
        /// </summary>
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

            var errors = new List<string>();

            var instanceType = step.GetType();

            var variableNameProperties1 = instanceType
                .GetProperties()
                .Where(x => x.PropertyType == typeof(VariableName))
                .Where(x => x.GetCustomAttribute<VariableNameAttribute>() != null);


            var simpleProperties1 = instanceType.GetProperties()
                .Where(x => x.GetCustomAttribute<StepPropertyAttribute>() != null);

            var listProperties1 = instanceType.GetProperties()
                .Where(x => x.GetCustomAttribute<StepListPropertyAttribute>() != null);

            var remainingProperties =
                variableNameProperties1.Select(propertyInfo => (propertyInfo,memberType: MemberType.VariableName))
                    .Concat(simpleProperties1.Select(propertyInfo => (propertyInfo,memberType: MemberType.Step)))
                    .Concat(listProperties1.Select(propertyInfo => (propertyInfo,memberType: MemberType.StepList)))
                    .ToDictionary(x=>x.propertyInfo.Name, StringComparer.OrdinalIgnoreCase);


            foreach (var (propertyName, stepMember) in freezableStepData.Dictionary)
            {
#pragma warning disable 8714
                if (remainingProperties.Remove(propertyName, out var pair))
#pragma warning restore 8714
                {
                    var convertResult = stepMember.TryConvert(pair.memberType);
                    if(convertResult.IsFailure)
                        errors.Add(convertResult.Error);
                    else
                    {
                        var result = pair.memberType switch
                        {
                            MemberType.VariableName => TrySetVariableName(pair.propertyInfo, step,
                                convertResult.Value),
                            MemberType.Step => TrySetStep(pair.propertyInfo, step, convertResult.Value,
                                stepContext),
                            MemberType.StepList => TrySetStepList(pair.propertyInfo, step,
                                convertResult.Value, stepContext),
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        if(result.IsFailure)
                            errors.Add(result.Error);
                    }
                }
                else
                    errors.Add($"The property '{propertyName}' does not exist on type '{TypeName}'.");



                static Result TrySetVariableName(PropertyInfo propertyInfo, IStep parentStep, StepMember member)
                {
                    var r1 = member.AsVariableName(propertyInfo.Name);
                    if (r1.IsFailure) return r1;

                    propertyInfo.SetValue(parentStep, r1.Value);
                    return Result.Success();
                }

                static Result TrySetStep(PropertyInfo propertyInfo, IStep parentStep, StepMember member, StepContext context)
                {
                    var argumentFreezeResult = member.AsArgument(propertyInfo.Name).Bind(x=>x.TryFreeze(context));
                    if (argumentFreezeResult.IsFailure)
                        return argumentFreezeResult;
                    if (!propertyInfo.PropertyType.IsInstanceOfType(argumentFreezeResult.Value))
                        return Result.Failure($"'{propertyInfo.Name}' cannot take the value '{argumentFreezeResult.Value}'");

                    propertyInfo.SetValue(parentStep, argumentFreezeResult.Value); //This could throw an exception but we don't expect it.
                    return Result.Success();
                }

                static Result TrySetStepList(PropertyInfo propertyInfo, IStep parentStep, StepMember member, StepContext context)
                {
                    var freezeResult =
                        member
                            .AsListArgument(propertyInfo.Name)
                            .Bind(l => l.Select(x => x.TryFreeze(context)).Combine()
                                .Map(x => x.ToImmutableArray()));
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
                            return Result.Failure($"'{step1.Name}' does not have the type '{genericType.Name}'");


                    propertyInfo.SetValue(parentStep, list);

                    return Result.Success();
                }
            }

            errors.AddRange(remainingProperties.Values
                .Where(property => property.propertyInfo.GetCustomAttribute<RequiredAttribute>() != null)
                .Select(property => $"The property '{property.propertyInfo.Name}' was not set on type '{GetType().Name}'.")
            );


            if (errors.Any())
                return Result.Failure<IStep>(string.Join("\r\n", errors));

            return Result.Success<IStep>(step);

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
    }
}