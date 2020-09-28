using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Serialization;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// A factory for creating runnable processes.
    /// </summary>
    public abstract class StepFactory : IStepFactory
    {
        /// <inheritdoc />
        public abstract Result<ITypeReference> TryGetOutputTypeReference(FreezableStepData freezableStepData);

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
        /// Gets all enum types used by this RunnableProcess.
        /// </summary>
        public abstract IEnumerable<Type> EnumTypes { get; }

        /// <inheritdoc />
        public abstract string OutputTypeExplanation { get; }


        /// <inheritdoc />
        public virtual Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName, FreezableStepData freezableStepData) =>
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
        public Result<IStep> TryFreeze(StepContext stepContext, FreezableStepData freezableStepData, Configuration? processConfiguration)
        {
            var instanceResult = TryCreateInstance(stepContext, freezableStepData);

            if (instanceResult.IsFailure)
                return instanceResult.ConvertFailure<IStep>();

            var runnableProcess = instanceResult.Value;
            runnableProcess.Configuration = processConfiguration;

            var errors = new List<string>();

            var instanceType = runnableProcess.GetType();

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


            foreach (var (propertyName, processMember) in freezableStepData.Dictionary)
            {
#pragma warning disable 8714
                if (remainingProperties.Remove(propertyName, out var pair))
#pragma warning restore 8714
                {
                    var convertResult = processMember.TryConvert(pair.memberType);
                    if(convertResult.IsFailure)
                        errors.Add(convertResult.Error);
                    else
                    {
                        var result = pair.memberType switch
                        {
                            MemberType.VariableName => TrySetVariableName(pair.propertyInfo, runnableProcess,
                                convertResult.Value),
                            MemberType.Step => TrySetProcess(pair.propertyInfo, runnableProcess, convertResult.Value,
                                stepContext),
                            MemberType.StepList => TrySetProcessList(pair.propertyInfo, runnableProcess,
                                convertResult.Value, stepContext),
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        if(result.IsFailure)
                            errors.Add(result.Error);
                    }
                }
                else
                    errors.Add($"The property '{propertyName}' does not exist on type '{TypeName}'.");



                static Result TrySetVariableName(PropertyInfo propertyInfo, IStep parentProcess, StepMember processMember)
                {
                    var r1 = processMember.AsVariableName(propertyInfo.Name);
                    if (r1.IsFailure) return r1;

                    propertyInfo.SetValue(parentProcess, r1.Value);
                    return Result.Success();
                }

                static Result TrySetProcess(PropertyInfo propertyInfo, IStep parentProcess, StepMember processMember, StepContext processContext)
                {
                    var argumentFreezeResult = processMember.AsArgument(propertyInfo.Name).Bind(x=>x.TryFreeze(processContext));
                    if (argumentFreezeResult.IsFailure)
                        return argumentFreezeResult;
                    if (!propertyInfo.PropertyType.IsInstanceOfType(argumentFreezeResult.Value))
                        return Result.Failure($"'{propertyInfo.Name}' cannot take the value '{argumentFreezeResult.Value}'");

                    propertyInfo.SetValue(parentProcess, argumentFreezeResult.Value); //This could throw an exception but we don't expect it.
                    return Result.Success();
                }

                static Result TrySetProcessList(PropertyInfo propertyInfo, IStep parentProcess, StepMember processMember, StepContext processContext)
                {
                    var freezeResult =
                        processMember
                            .AsListArgument(propertyInfo.Name)
                            .Bind(l => l.Select(x => x.TryFreeze(processContext)).Combine()
                                .Map(x => x.ToImmutableArray()));
                    if (freezeResult.IsFailure)
                        return freezeResult;

                    var genericType = propertyInfo.PropertyType.GenericTypeArguments.Single();
                    var listType = typeof(List<>).MakeGenericType(genericType);

                    var list = Activator.CreateInstance(listType);

                    foreach (var process in freezeResult.Value)
                        if (genericType.IsInstanceOfType(process))
                        {
                            var addMethod = listType.GetMethod(nameof(List<object>.Add))!;
                            addMethod.Invoke(list, new object?[] { process });
                        }
                        else
                            return Result.Failure($"'{process.Name}' does not have the type '{genericType.Name}'");


                    propertyInfo.SetValue(parentProcess, list);

                    return Result.Success();
                }
            }

            errors.AddRange(remainingProperties.Values
                .Where(property => property.propertyInfo.GetCustomAttribute<RequiredAttribute>() != null)
                .Select(property => $"The property '{property.propertyInfo.Name}' was not set on type '{GetType().Name}'.")
            );


            if (errors.Any())
                return Result.Failure<IStep>(string.Join("\r\n", errors));

            return Result.Success<IStep>(runnableProcess);

        }


        /// <summary>
        /// Creates a typed generic IRunnableProcess with one type argument.
        /// </summary>
        protected static Result<ICompoundStep> TryCreateGeneric(Type openGenericType, Type parameterType)
        {
            var genericType = openGenericType.MakeGenericType(parameterType);

            var r = Activator.CreateInstance(genericType);

            if (r is ICompoundStep rp)
                return Result.Success(rp);

            return Result.Failure<ICompoundStep>($"Could not create an instance of {openGenericType.Name}<{parameterType.Name}>");
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