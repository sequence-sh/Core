using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// The name of a variable that can be written and read from the process state.
    /// </summary>
    public readonly struct VariableName : IEquatable<VariableName>
    {
        /// <summary>
        /// Creates a new VariableName.
        /// </summary>
        public VariableName(string name) => Name = name;

        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string Name { get;  }

        /// <inheritdoc />
        public bool Equals(VariableName other) => Name == other.Name;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is VariableName other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Name.GetHashCode();

        /// <summary>
        /// Equals operator
        /// </summary>
        public static bool operator ==(VariableName left, VariableName right) => left.Equals(right);

        /// <summary>
        /// Not Equals Operator
        /// </summary>
        public static bool operator !=(VariableName left, VariableName right) => !left.Equals(right);

        /// <summary>
        /// Creates the name of a generic type argument.
        /// </summary>
        public VariableName CreateChild(int argNumber) => new VariableName(Name + "ARG" + argNumber);

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>
    /// Any member of a process.
    /// </summary>
    public sealed class ProcessMember
    {
        public ProcessMember(VariableName variableName) => VariableName = variableName;
        public ProcessMember(IFreezableProcess argument) => Argument = argument;
        public ProcessMember(IReadOnlyList<IFreezableProcess> listArgument) => ListArgument = listArgument;


        /// <summary>
        /// The member type of this Process Member.
        /// </summary>
        public MemberType MemberType
        {
            get
            {
                if (VariableName.HasValue) return MemberType.VariableName;
                else if (Argument != null) return MemberType.Process;
                else if (ListArgument != null) return MemberType.ProcessList;

                return MemberType.NotAMember;
            }
        }

        public VariableName? VariableName { get; set; }

        public IFreezableProcess? Argument { get; set; }

        public IReadOnlyList<IFreezableProcess>? ListArgument { get; set; }


        public T Join<T>(Func<VariableName, T> handleVariableName, Func<IFreezableProcess, T> handleArgument,
            Func<IReadOnlyList<IFreezableProcess>, T> handleListArgument)
        {
            if (VariableName != null) return handleVariableName(VariableName.Value);

            if(Argument!= null) return handleArgument(Argument);

            if(ListArgument != null) return handleListArgument(ListArgument);

            throw new Exception("Process Member has no property");
        }

        public Result<VariableName> AsVariableName(string propertyName)
        {
            if (VariableName != null) return VariableName.Value;
            return Result.Failure<VariableName>($"{propertyName} was not a VariableName");
        }

        public Result<IFreezableProcess> AsArgument(string propertyName)
        {
            if (Argument != null) return Result.Success(Argument);
            return Result.Failure<IFreezableProcess>($"{propertyName} was not an argument");
        }

        public Result<IReadOnlyList<IFreezableProcess>> AsListArgument(string propertyName)
        {
            if (ListArgument != null) return Result.Success(ListArgument);
            return Result.Failure<IReadOnlyList<IFreezableProcess>>($"{propertyName} was not an list argument");
        }

        /// <summary>
        /// A string representation of the member.
        /// </summary>
        public string MemberString => VariableName?.ToString()??Argument?.ToString()??ListArgument?.ToString()??"Unknown";

        /// <inheritdoc />
        public override string ToString() => new {MemberType, Value=MemberString}.ToString()!;
    }

    /// <summary>
    /// The data used by a Freezable Process.
    /// </summary>
    public sealed class FreezableProcessData
    {
        ///// <summary>
        ///// Create a new FreezableProcessData
        ///// </summary>
        public FreezableProcessData(IReadOnlyDictionary<string, ProcessMember> dictionary) => Dictionary = dictionary;

        public IReadOnlyDictionary<string, ProcessMember> Dictionary { get; }

        public Result<VariableName> GetVariableName(string name) =>
            Dictionary.TryFindOrFail(name, null).Bind(x => x.AsVariableName(name));

        public Result<IFreezableProcess> GetArgument(string name) =>
            Dictionary.TryFindOrFail(name, null).Bind(x => x.AsArgument(name));

        public Result<IReadOnlyList<IFreezableProcess>> GetListArgument(string name) =>
            Dictionary.TryFindOrFail(name, null).Bind(x => x.AsListArgument(name));
    }

    /// <summary>
    /// A process that is not a constant or a variable reference.
    /// </summary>
    public sealed class CompoundFreezableProcess : IFreezableProcess
    {
        /// <summary>
        /// Creates a new CompoundFreezableProcess.
        /// </summary>
        public CompoundFreezableProcess(RunnableProcessFactory processFactory, FreezableProcessData freezableProcessData)
        {
            ProcessFactory = processFactory;
            FreezableProcessData = freezableProcessData;
        }


        /// <summary>
        /// The factory for this process.
        /// </summary>
        public RunnableProcessFactory ProcessFactory { get; }

        /// <summary>
        /// The data for this process.
        /// </summary>
        public FreezableProcessData FreezableProcessData { get; }


        /// <inheritdoc />
        public Result<IRunnableProcess> TryFreeze(ProcessContext processContext) => ProcessFactory.TryFreeze(processContext, FreezableProcessData);

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(VariableName VariableName, ITypeReference type)>> TryGetVariablesSet
        {
            get
            {
                var result = FreezableProcessData
                    .Dictionary.Values
                    .Select(TryGetProcessMemberVariablesSet)
                    .Combine()
                    .Map(x=>x.SelectMany(y=>y).ToList() as IReadOnlyCollection<(VariableName name, ITypeReference type)>);

                return result;


                 Result<IReadOnlyCollection<(VariableName, ITypeReference)>> TryGetProcessMemberVariablesSet(ProcessMember processMember) =>
                     processMember.Join(vn =>
                             ProcessFactory.GetTypeReferencesSet(vn, FreezableProcessData)
                                 .Map(y=> y.Map(x => new[] { (vn, x) } as IReadOnlyCollection<(VariableName, ITypeReference)>)
                                 .Unwrap(ImmutableArray<(VariableName, ITypeReference)>.Empty)),
                         y => y.TryGetVariablesSet,
                         y => y.Select(z => z.TryGetVariablesSet).Combine().Map(x =>
                             x.SelectMany(y => y).ToList() as IReadOnlyCollection<(VariableName, ITypeReference)>));
            }
        }



        /// <inheritdoc />
        public string ProcessName => ProcessFactory.ProcessNameBuilder.GetFromArguments(FreezableProcessData);

        /// <inheritdoc />
        public Result<ITypeReference> TryGetOutputTypeReference() => ProcessFactory.TryGetOutputTypeReference(FreezableProcessData);

        /// <inheritdoc />
        public override string ToString() => ProcessName;
    }

    /// <summary>
    /// The type of a process member.
    /// </summary>
    public enum MemberType
    {
        NotAMember,
        VariableName,
        Process,
        ProcessList

    }

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
        /// Creates an instance of this type.
        /// </summary>
        protected abstract Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, FreezableProcessData freezableProcessData);

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

            if (instanceResult.IsFailure) return instanceResult;

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

            return Result.Success(runnableProcess);

        }


        /// <summary>
        /// Creates a typed generic IRunnableProcess with one type argument.
        /// </summary>
        protected static Result<IRunnableProcess> TryCreateGeneric(Type openGenericType, Type parameterType)
        {
            var genericType = openGenericType.MakeGenericType(parameterType);

            var r = Activator.CreateInstance(genericType);

            if (r is IRunnableProcess rp)
                return Result.Success(rp);

            return Result.Failure<IRunnableProcess>($"Could not create an instance of {openGenericType.Name}<{parameterType.Name}>");
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