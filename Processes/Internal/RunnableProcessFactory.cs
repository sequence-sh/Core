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
    public interface IRunnableProcessFactory
    {
        /// <summary>
        /// Unique name for this type of process.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// The type of the process to create.
        /// </summary>
        public Type ProcessType { get; }


        /// <summary>
        /// Builds the name for a particular instance of a process.
        /// </summary>
        IProcessNameBuilder ProcessNameBuilder { get; }

        /// <summary>
        /// Tries to get a reference to the output type of this process.
        /// </summary>
        Result<ITypeReference> TryGetOutputTypeReference(FreezableProcessData freezableProcessData);

        /// <summary>
        /// If this variable is being set. Get the type reference it is being set to.
        /// </summary>
        Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName, FreezableProcessData freezableProcessData) =>
            Maybe<ITypeReference>.None;

        /// <summary>
        /// Serializer to use for yaml serialization.
        /// </summary>
        IProcessSerializer Serializer { get; }

        /// <summary>
        /// An object which can combine a process with the next process in the sequence.
        /// </summary>
        Maybe<IProcessCombiner> ProcessCombiner { get; }

        /// <summary>
        /// Special requirements for this process.
        /// </summary>
        IEnumerable<Requirement> Requirements { get; }

        /// <summary>
        /// Try to create the instance of this type and set all arguments.
        /// </summary>
        Result<IRunnableProcess> TryFreeze(ProcessContext processContext, FreezableProcessData freezableProcessData,
            ProcessConfiguration? processConfiguration);

        /// <summary>
        /// Human readable explanation of the output type.
        /// </summary>
        string OutputTypeExplanation { get; }
    }



    /// <summary>
    /// A factory for creating runnable processes.
    /// </summary>
    public abstract class RunnableProcessFactory : IRunnableProcessFactory
    {
        /// <inheritdoc />
        public abstract Result<ITypeReference> TryGetOutputTypeReference(FreezableProcessData freezableProcessData);

        /// <inheritdoc />
        public string TypeName => FormatTypeName(ProcessType);

        /// <summary>
        /// The type of this process.
        /// </summary>
        public abstract Type ProcessType { get; }


        /// <inheritdoc />
        public override string ToString() => TypeName;

        /// <inheritdoc />
        public abstract IProcessNameBuilder ProcessNameBuilder { get; }

        /// <summary>
        /// Gets all enum types used by this RunnableProcess.
        /// </summary>
        public abstract IEnumerable<Type> EnumTypes { get; }

        /// <inheritdoc />
        public abstract string OutputTypeExplanation { get; }


        /// <inheritdoc />
        public virtual Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName, FreezableProcessData freezableProcessData) =>
            Maybe<ITypeReference>.None;

        /// <inheritdoc />
        public virtual IProcessSerializer Serializer => new FunctionSerializer(TypeName);


        /// <inheritdoc />
        public virtual Maybe<IProcessCombiner> ProcessCombiner => Maybe<IProcessCombiner>.None;

        /// <inheritdoc />
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
            var propertyInfo = ProcessType.GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null) return MemberType.NotAMember;

            if (propertyInfo.GetCustomAttribute<VariableNameAttribute>() != null) return MemberType.VariableName;
            if (propertyInfo.GetCustomAttribute<RunnableProcessPropertyAttributeAttribute>() != null) return MemberType.Process;
            if (propertyInfo.GetCustomAttribute<RunnableProcessListPropertyAttributeAttribute>() != null) return MemberType.ProcessList;

            return MemberType.NotAMember;

        }


        /// <inheritdoc />
        public Result<IRunnableProcess> TryFreeze(ProcessContext processContext, FreezableProcessData freezableProcessData, ProcessConfiguration? processConfiguration)
        {
            var instanceResult = TryCreateInstance(processContext, freezableProcessData);

            if (instanceResult.IsFailure)
                return instanceResult.ConvertFailure<IRunnableProcess>();

            var runnableProcess = instanceResult.Value;
            runnableProcess.ProcessConfiguration = processConfiguration;

            var errors = new List<string>();

            var instanceType = runnableProcess.GetType();

            var variableNameProperties1 = instanceType
                .GetProperties()
                .Where(x => x.PropertyType == typeof(VariableName))
                .Where(x => x.GetCustomAttribute<VariableNameAttribute>() != null);


            var simpleProperties1 = instanceType.GetProperties()
                .Where(x => x.GetCustomAttribute<RunnableProcessPropertyAttributeAttribute>() != null);

            var listProperties1 = instanceType.GetProperties()
                .Where(x => x.GetCustomAttribute<RunnableProcessListPropertyAttributeAttribute>() != null);

            var remainingProperties =
                variableNameProperties1.Select(propertyInfo => (propertyInfo,memberType: MemberType.VariableName))
                    .Concat(simpleProperties1.Select(propertyInfo => (propertyInfo,memberType: MemberType.Process)))
                    .Concat(listProperties1.Select(propertyInfo => (propertyInfo,memberType: MemberType.ProcessList)))
                    .ToDictionary(x=>x.propertyInfo.Name, StringComparer.OrdinalIgnoreCase);


            foreach (var (propertyName, processMember) in freezableProcessData.Dictionary)
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
                            MemberType.Process => TrySetProcess(pair.propertyInfo, runnableProcess, convertResult.Value,
                                processContext),
                            MemberType.ProcessList => TrySetProcessList(pair.propertyInfo, runnableProcess,
                                convertResult.Value, processContext),
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        if(result.IsFailure)
                            errors.Add(result.Error);
                    }
                }
                else
                    errors.Add($"The property '{propertyName}' does not exist on type '{TypeName}'.");



                static Result TrySetVariableName(PropertyInfo propertyInfo, IRunnableProcess parentProcess, ProcessMember processMember)
                {
                    var r1 = processMember.AsVariableName(propertyInfo.Name);
                    if (r1.IsFailure) return r1;

                    propertyInfo.SetValue(parentProcess, r1.Value);
                    return Result.Success();
                }

                static Result TrySetProcess(PropertyInfo propertyInfo, IRunnableProcess parentProcess, ProcessMember processMember, ProcessContext processContext)
                {
                    var argumentFreezeResult = processMember.AsArgument(propertyInfo.Name).Bind(x=>x.TryFreeze(processContext));
                    if (argumentFreezeResult.IsFailure)
                        return argumentFreezeResult;
                    if (!propertyInfo.PropertyType.IsInstanceOfType(argumentFreezeResult.Value))
                        return Result.Failure($"'{propertyInfo.Name}' cannot take the value '{argumentFreezeResult.Value}'");

                    propertyInfo.SetValue(parentProcess, argumentFreezeResult.Value); //This could throw an exception but we don't expect it.
                    return Result.Success();
                }

                static Result TrySetProcessList(PropertyInfo propertyInfo, IRunnableProcess parentProcess, ProcessMember processMember, ProcessContext processContext)
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