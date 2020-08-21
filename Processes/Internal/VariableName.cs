using System;

namespace Reductech.EDR.Processes.Internal
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
}