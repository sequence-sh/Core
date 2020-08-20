using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Serialization;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// A factory for creating runnable processes.
    /// </summary>
    public abstract class RunnableProcessFactory
    {
        /// <summary>
        /// Tries to get a reference to the output type of this process.
        /// </summary>
        public abstract Result<ITypeReference> TryGetOutputTypeReference(FreezableProcessData freezableProcessData);

        /// <summary>
        /// Unique name for this type of process.
        /// </summary>
        public string TypeName => FormatTypeName(ProcessType);

        /// <summary>
        /// The type of this process.
        /// </summary>
        public abstract Type ProcessType { get; }


        /// <inheritdoc />
        public override string ToString() => TypeName;

        /// <summary>
        /// Builds the name for a particular instance of a process.
        /// </summary>
        public abstract IProcessNameBuilder ProcessNameBuilder { get; }

        /// <summary>
        /// Gets all enum types used by this RunnableProcess.
        /// </summary>
        public abstract IEnumerable<Type> EnumTypes { get; }

        /// <summary>
        /// If this variable is being set. Get the type reference it is being set to.
        /// </summary>
        public virtual Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName, FreezableProcessData freezableProcessData) =>
            Maybe<ITypeReference>.None;

        /// <summary>
        /// Custom serializers to use for yaml serialization and deserialization.
        /// </summary>
        public virtual Maybe<ICustomSerializer> CustomSerializer { get; } = Maybe<ICustomSerializer>.None;


        /// <summary>
        /// Special requirements for this process.
        /// </summary>
        public virtual IEnumerable<Requirement> Requirements => ImmutableArray<Requirement>.Empty;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        protected abstract Result<ICompoundRunnableProcess> TryCreateInstance(ProcessContext processContext, FreezableProcessData freezableProcessData);

        /// <summary>
        /// Gets the type of this member.
        /// </summary>
        public MemberType GetExpectedMemberType(string name)
        {
            var propertyInfo = ProcessType.GetProperty(name);

            if (propertyInfo == null) return MemberType.NotAMember;

            if (propertyInfo.GetCustomAttribute<VariableNameAttribute>() != null) return MemberType.VariableName;
            if (propertyInfo.GetCustomAttribute<RunnableProcessPropertyAttribute>() != null) return MemberType.Process;
            if (propertyInfo.GetCustomAttribute<RunnableProcessListPropertyAttribute>() != null) return MemberType.ProcessList;

            return MemberType.NotAMember;

        }


        /// <summary>
        /// Try to create the instance of this type and set all arguments.
        /// </summary>
        public Result<IRunnableProcess> TryFreeze(ProcessContext processContext, FreezableProcessData freezableProcessData)
        {
            var instanceResult = TryCreateInstance(processContext, freezableProcessData);

            if (instanceResult.IsFailure) return instanceResult.Map(x=>x as IRunnableProcess);

            instanceResult.Value.ProcessConfiguration = freezableProcessData.ProcessConfiguration;

            var runnableProcess = instanceResult.Value;

            var errors = new List<string>();

            var instanceType = instanceResult.Value.GetType();

            var remainingVariableNameProperties = instanceType
                .GetProperties()
                .Where(x => x.PropertyType == typeof(VariableName))
                .Where(x => x.GetCustomAttribute<VariableNameAttribute>() != null)
                .ToDictionary(x=>x.Name);


            var remainingProperties = instanceType.GetProperties()
                .Where(x => x.GetCustomAttribute<RunnableProcessPropertyAttribute>() != null)
                .ToDictionary(x => x.Name);

            var remainingListProperties = instanceType.GetProperties()
                .Where(x => x.GetCustomAttribute<RunnableProcessListPropertyAttribute>() != null)
                .ToDictionary(x => x.Name);


            foreach (var (propertyName, processMember) in freezableProcessData.Dictionary)
            {
                Result SetVariableName(VariableName variableName)
                {
                    if (remainingVariableNameProperties.Remove(propertyName, out var pi))
                        pi.SetValue(instanceResult.Value, variableName);
                    else
                        return Result.Failure($"The property '{propertyName}' does not exist on type '{TypeName}'.");
                    return Result.Success();
                }

                Result SetArgument(IFreezableProcess freezableProcess)
                {
                    if (remainingProperties.Remove(propertyName, out var pi))
                    {
                        var argumentFreezeResult = freezableProcess.TryFreeze(processContext);
                        if (argumentFreezeResult.IsFailure)
                            errors.Add(argumentFreezeResult.Error);
                        else
                        {
                            if (pi.PropertyType.IsInstanceOfType(argumentFreezeResult.Value))
                                pi.SetValue(runnableProcess, argumentFreezeResult.Value); //This could throw an exception but we don't expect it.
                            else
                                return Result.Failure($"'{pi.Name}' cannot take the value '{argumentFreezeResult.Value}'");
                        }
                    }
                    else
                        return Result.Failure($"The property '{propertyName}' does not exist on type '{TypeName}'.");

                    return Result.Success();
                }

                Result SetArgumentList(IReadOnlyList<IFreezableProcess> processList)
                {
                    if (remainingListProperties.Remove(propertyName, out var listInfo))
                    {
                        var freezeResult = processList.Select(x => x.TryFreeze(processContext)).Combine()
                            .Map(x => x.ToImmutableArray());
                        if (freezeResult.IsFailure)
                            return freezeResult;

                        var genericType = listInfo.PropertyType.GenericTypeArguments.Single();
                        var listType = typeof(List<>).MakeGenericType(genericType);

                        var list = Activator.CreateInstance(listType);

                        foreach (var process in freezeResult.Value)
                            if (genericType.IsInstanceOfType(process))
                            {
                                var addMethod = listType.GetMethod(nameof(List<object>.Add))!;
                                addMethod.Invoke(list, new object?[] {process});
                            }
                            else
                                return Result.Failure($"'{process.Name}' does not have the type '{genericType.Name}'");


                        listInfo.SetValue(runnableProcess, list);

                        return Result.Success();
                    }
                    else
                        return Result.Failure($"The property '{propertyName}' does not exist on type '{TypeName}'.");
                }

                var r = processMember.Join(SetVariableName, SetArgument, SetArgumentList);
                if(r.IsFailure) errors.Add(r.Error);
            }


            errors.AddRange(remainingVariableNameProperties.Values
                .Where(property => property.GetCustomAttribute<RequiredAttribute>() != null)
                .Select(property => $"The property '{property.Name}' was not set on type '{GetType().Name}'."));

            errors.AddRange(remainingProperties.Values
                .Where(property => property.GetCustomAttribute<RequiredAttribute>() != null)
                .Select(property => $"The property '{property.Name}' was not set on type '{GetType().Name}'."));

            errors.AddRange(remainingListProperties.Values
                .Where(property => property.GetCustomAttribute<RequiredAttribute>() != null)
                .Select(property => $"The property '{property.Name}' was not set on type '{GetType().Name}'."));


            if (errors.Any())
                return Result.Failure<IRunnableProcess>(string.Join("\r\n", errors));

            return Result.Success<IRunnableProcess>(runnableProcess);

        }


        /// <summary>
        /// Creates a typed generic IRunnableProcess with one type argument.
        /// </summary>
        protected static Result<ICompoundRunnableProcess> TryCreateGeneric(Type openGenericType, Type parameterType)
        {
            var genericType = openGenericType.MakeGenericType(parameterType);

            var r = Activator.CreateInstance(genericType);

            if (r is ICompoundRunnableProcess rp)
                return Result.Success(rp);

            return Result.Failure<ICompoundRunnableProcess>($"Could not create an instance of {openGenericType.Name}<{parameterType.Name}>");
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