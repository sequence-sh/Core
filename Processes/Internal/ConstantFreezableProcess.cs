using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// A process that returns a fixed value when run.
    /// </summary>
    public sealed class ConstantFreezableProcess : IFreezableProcess, IEquatable<ConstantFreezableProcess>
    {
        /// <summary>
        /// Creates a new ConstantFreezableProcess.
        /// </summary>
        /// <param name="value"></param>
        public ConstantFreezableProcess(object value) => Value = value;

        /// <summary>
        /// The value that this will return when run.
        /// </summary>
        public object Value { get; }


        /// <inheritdoc />
        public Result<IRunnableProcess> TryFreeze(ProcessContext _)
        {
            Type elementType = Value.GetType();
            Type processType = typeof(Constant<>).MakeGenericType(elementType);
            var process = Activator.CreateInstance(processType, Value);

            //TODO check for exceptions here?

            var runnableProcess = (IRunnableProcess) process!;

            return Result.Success(runnableProcess);
        }

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(VariableName VariableName, ITypeReference type)>> TryGetVariablesSet => ImmutableList<(VariableName VariableName, ITypeReference type)>.Empty;

        /// <inheritdoc />
        public string ProcessName
        {
            get
            {
                if (Value is string s)
                    return $"'{s}'";

                if (Value is Enum e)
                    return e.GetDisplayName();

                return $"{Value}";
            }
        }

        /// <inheritdoc />
        public Result<ITypeReference> TryGetOutputTypeReference() => new ActualTypeReference(Value.GetType());

        /// <inheritdoc />
        public override string ToString() => ProcessName;

        /// <inheritdoc />
        public bool Equals(ConstantFreezableProcess? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value.Equals(other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ConstantFreezableProcess other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary>
        /// Equals operator.
        /// </summary>
        public static bool operator ==(ConstantFreezableProcess? left, ConstantFreezableProcess? right) => Equals(left, right);

        /// <summary>
        /// Not Equals operator
        /// </summary>
        public static bool operator !=(ConstantFreezableProcess? left, ConstantFreezableProcess? right) => !Equals(left, right);
    }
}