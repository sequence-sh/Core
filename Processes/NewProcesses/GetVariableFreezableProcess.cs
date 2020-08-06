using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// Either a type itself, or the name of a variable with the same type.
    /// </summary>
    public interface ITypeReference
    {
        /// <summary>
        /// Gets the variable type references.
        /// </summary>
        IEnumerable<VariableTypeReference> VariableTypeReferences { get; }

        /// <summary>
        /// Gets the actual type references.
        /// </summary>
        IEnumerable<ActualTypeReference> ActualTypeReferences { get; }

        IEnumerable<ITypeReference> TypeArgumentReferences { get; }

    }

    /// <summary>
    /// An instance of a generic type.
    /// </summary>
    public sealed class GenericTypeReference : ITypeReference, IEquatable<ITypeReference>
    {
        /// <summary>
        /// Create a new GenericTypeReference.
        /// </summary>
        public GenericTypeReference(Type genericType, IReadOnlyList<ITypeReference> childTypes)
        {
            GenericType = genericType;
            ChildTypes = childTypes;
        }

        /// <inheritdoc />
        public IEnumerable<VariableTypeReference> VariableTypeReferences => ImmutableList<VariableTypeReference>.Empty;

        /// <inheritdoc />
        public IEnumerable<ActualTypeReference> ActualTypeReferences => ImmutableList<ActualTypeReference>.Empty;

        /// <inheritdoc />
        public IEnumerable<ITypeReference> TypeArgumentReferences => ChildTypes;

        /// <summary>
        /// The generic type.
        /// </summary>
        public Type GenericType { get; }

        /// <summary>
        /// The generic type members.
        /// </summary>
        public IReadOnlyList<ITypeReference> ChildTypes { get; }


        /// <inheritdoc />
        public bool Equals(ITypeReference? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return other is GenericTypeReference gtr && GenericType == gtr.GenericType &&
                   ChildTypes.SequenceEqual(gtr.ChildTypes);
        }
    }

    /// <summary>
    /// An actual instance of the type.
    /// </summary>
    public sealed class ActualTypeReference : ITypeReference, IEquatable<ITypeReference>
    {
        /// <summary>
        /// Creates a new ActualTypeReference.
        /// </summary>
        public ActualTypeReference(Type type) => Type = type;

        /// <summary>
        /// The type to use.
        /// </summary>
        public Type Type { get; }

        /// <inheritdoc />
        public bool Equals(ITypeReference? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return other switch
            {
                ActualTypeReference actualType => Type == actualType.Type,
                MultipleTypeReference multipleTypeReference => multipleTypeReference.AllReferences.Count == 0 &&
                                                               multipleTypeReference.AllReferences.Contains(this),
                VariableTypeReference _ => false,
                GenericTypeReference _ => false,
                _ => throw new ArgumentOutOfRangeException(nameof(other))
            };
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ITypeReference other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Type.GetHashCode();

        /// <inheritdoc />
        IEnumerable<VariableTypeReference> ITypeReference.VariableTypeReferences => ImmutableArray<VariableTypeReference>.Empty;

        /// <inheritdoc />
        IEnumerable<ActualTypeReference> ITypeReference.ActualTypeReferences => new[] {this};

        /// <inheritdoc />
        public IEnumerable<ITypeReference> TypeArgumentReferences => ImmutableList<ITypeReference>.Empty;
    }

    /// <summary>
    /// Indicates that this type is the same as that of the variable with the given name.
    /// </summary>
    public sealed class VariableTypeReference : ITypeReference, IEquatable<ITypeReference>
    {
        /// <summary>
        /// Creates a new VariableTypeReference.
        /// </summary>
        /// <param name="variableName"></param>
        public VariableTypeReference(string variableName) => VariableName = variableName;

        /// <summary>
        /// The name of a variable with the same type as this type.
        /// </summary>
        public string VariableName { get; }

        /// <inheritdoc />
        public bool Equals(ITypeReference? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return other switch
            {
                ActualTypeReference _ => false,
                MultipleTypeReference multipleTypeReference => multipleTypeReference.AllReferences.Count == 1 &&
                                                               multipleTypeReference.AllReferences.Contains(this),
                VariableTypeReference typeReference => VariableName == typeReference.VariableName,
                GenericTypeReference _ => false,
                _ => throw new ArgumentOutOfRangeException(nameof(other))
            };
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ITypeReference other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => VariableName.GetHashCode();

        /// <inheritdoc />
        IEnumerable<VariableTypeReference> ITypeReference.VariableTypeReferences => new[] {this};

        /// <inheritdoc />
        IEnumerable<ActualTypeReference> ITypeReference.ActualTypeReferences => ImmutableArray<ActualTypeReference>.Empty;

        /// <inheritdoc />
        public IEnumerable<ITypeReference> TypeArgumentReferences => ImmutableList<ITypeReference>.Empty;
    }

    /// <summary>
    /// A type that is the same a multiple different references.
    /// </summary>
    public sealed class MultipleTypeReference : ITypeReference, IEquatable<ITypeReference>
    {
        /// <summary>
        /// Tries to create a new MultipleTypeReference.
        /// </summary>
        public static Result<ITypeReference> TryCreate(IEnumerable<ITypeReference> references, string parentProcess)
        {
            var set = references.ToImmutableHashSet();

            switch (set.Count)
            {
                case 0:
                    return Result.Failure<ITypeReference>($"Could not infer type for {parentProcess} as it has no children.");
                case 1:
                    return Result.Success(set.Single());
                default:
                {
                    if (set.OfType<ActualTypeReference>().Count() > 1)
                        return Result.Failure<ITypeReference>(
                            $"Could not infer type for {parentProcess} as it's children have different types ({string.Join(", ", set.OfType<ActualTypeReference>().Select(x=>x.Type.Name))}).");
                    return new MultipleTypeReference(set);
                }
            }
        }

        /// <summary>
        /// Creates a new MultipleTypeReference.
        /// </summary>
        /// <param name="allReferences"></param>
        private MultipleTypeReference(ImmutableHashSet<ITypeReference> allReferences)
        {
            AllReferences = allReferences.ToImmutableHashSet();
        }

        /// <summary>
        /// The type references.
        /// </summary>
        public ImmutableHashSet<ITypeReference> AllReferences { get; }

        /// <inheritdoc />
        public bool Equals(ITypeReference? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return other switch
            {
                ActualTypeReference actualType => AllReferences.Count == 1 && AllReferences.Contains(actualType),
                MultipleTypeReference multipleTypeReference => AllReferences.SetEquals(multipleTypeReference
                    .AllReferences),
                VariableTypeReference variableTypeReference => AllReferences.Count == 1 && AllReferences.Contains(variableTypeReference),
                GenericTypeReference genericTypeReference => AllReferences.Count == 1 && AllReferences.Contains(genericTypeReference),
                _ => throw new ArgumentOutOfRangeException(nameof(other))
            };
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ITypeReference other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => AllReferences.FirstOrDefault().GetHashCode();

        /// <inheritdoc />
        public IEnumerable<VariableTypeReference> VariableTypeReferences => AllReferences.SelectMany(x=>x.VariableTypeReferences);

        /// <inheritdoc />
        public IEnumerable<ActualTypeReference> ActualTypeReferences => AllReferences.SelectMany(x=>x.ActualTypeReferences);

        /// <inheritdoc />
        public IEnumerable<ITypeReference> TypeArgumentReferences => ImmutableList<ITypeReference>.Empty;
    }



    /// <summary>
    /// Sets the value of a particular variable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SetVariableRunnableProcess<T> : IRunnableProcess<Unit>
    {
        /// <summary>
        /// Creates a new SetVariableRunnableProcess.
        /// </summary>
        public SetVariableRunnableProcess(string variableName, IRunnableProcess<T> value)
        {
            VariableName = variableName;
            Value = value;
        }

        /// <inheritdoc />
        public Result<Unit> Run(ProcessState processState)
        {
            var valueResult = Value.Run(processState);

            if (valueResult.IsFailure)
                return valueResult.ConvertFailure<Unit>();

            processState.SetVariable(VariableName, valueResult.Value);

            return Unit.Default;
        }

        /// <inheritdoc />
        public string Name => NameHelper.GetSetVariableName(VariableName, Value.Name);

        /// <summary>
        /// The name of the variable to set.
        /// </summary>
        public string VariableName { get; }

        /// <summary>
        /// The value that the variable will be set to.
        /// </summary>
        public IRunnableProcess<T> Value { get; }


        /// <inheritdoc />
        public IFreezableProcess Unfreeze() => new SetVariableFreezableProcess(VariableName, Value.Unfreeze());


        /// <inheritdoc />
        public override string ToString() => Name;
    }



    /// <summary>
    /// Sets the value of a particular variable.
    /// </summary>
    public sealed class SetVariableFreezableProcess : IFreezableProcess
    {
        /// <summary>
        /// Creates a new SetVariableFreezableProcess.
        /// </summary>
        public SetVariableFreezableProcess(string variableName, IFreezableProcess value)
        {
            VariableName = variableName;
            Value = value;
        }

        /// <inheritdoc />
        public Result<IRunnableProcess> TryFreeze(ProcessContext processContext)
        {
            var valueFreezeResult = Value.TryFreeze(processContext);

            if (valueFreezeResult.IsFailure)
                return valueFreezeResult.ConvertFailure<IRunnableProcess>();

            var outputTypeResult = Value.TryGetOutputTypeReference().Bind(processContext.TryGetTypeFromReference);

            if (outputTypeResult.IsFailure)
                return outputTypeResult.ConvertFailure<IRunnableProcess>();


            Type processType = typeof(SetVariableRunnableProcess<>).MakeGenericType(outputTypeResult.Value);
            var process = Activator.CreateInstance(processType, VariableName, valueFreezeResult.Value);

            //TODO check for exceptions here?

            var runnableProcess = (IRunnableProcess)process!;

            return Result.Success(runnableProcess);
        }

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(string name, ITypeReference type)>> TryGetVariablesSet
        {
            get
            {
                var result = Value.TryGetOutputTypeReference().Map(x => (VariableName, x)).Map(x => new[] {x} as IReadOnlyCollection<(string name, ITypeReference type)>);

                return result;
            }
        }

        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string VariableName { get; }

        /// <summary>
        /// The new value for the variable.
        /// </summary>
        public IFreezableProcess Value { get; }

        /// <inheritdoc />
        public string Name => NameHelper.GetSetVariableName(VariableName, Value.Name);

        /// <inheritdoc />
        public Result<ITypeReference> TryGetOutputTypeReference() => new ActualTypeReference(typeof(Unit));


        /// <inheritdoc />
        public override string ToString() => Name;
    }


    /// <summary>
    /// A process that gets the value of a particular variable.
    /// </summary>
    public sealed class GetVariableFreezableProcess : IFreezableProcess
    {
        /// <summary>
        /// Creates a new GetVariableFreezableProcess.
        /// </summary>
        /// <param name="variableName"></param>
        public GetVariableFreezableProcess(string variableName)
        {
            VariableName = variableName;
        }

        /// <summary>
        /// The name of the variable to get
        /// </summary>
        public string VariableName { get; }


        /// <inheritdoc />
        public Result<IRunnableProcess> TryFreeze(ProcessContext processContext)
        {
            if (!processContext.VariableTypesDictionary.TryGetValue(VariableName, out var type))
                return Result.Failure<IRunnableProcess>($"The variable '{VariableName}' is never set.");

            Type processType = typeof(GetVariableRunnableProcess<>).MakeGenericType(type);
            var process = Activator.CreateInstance(processType, VariableName);

            //TODO check for exceptions here?

            var runnableProcess = (IRunnableProcess)process!;

            return Result.Success(runnableProcess);
        }

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(string name, ITypeReference type)>> TryGetVariablesSet => ImmutableList<(string name, ITypeReference type)>.Empty;


        /// <inheritdoc />
        public string Name => NameHelper.GetGetVariableName(VariableName);

        /// <inheritdoc />
        public Result<ITypeReference> TryGetOutputTypeReference() => new VariableTypeReference(VariableName);

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}