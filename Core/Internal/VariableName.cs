using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// The name of a variable that can be written and read from the step state.
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
        public bool Equals(VariableName other)
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (Name == null)
                return other.Name == null;
            if (other.Name == null)
                return false;
            // ReSharper restore ConditionIsAlwaysTrueOrFalse


            return  Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is VariableName other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Name.ToLowerInvariant().GetHashCode();

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
        public override string ToString() => $"<{Name}>";

        /// <summary>
        /// Ensures that this variable name is not reserved
        /// </summary>
        /// <returns></returns>
        public Result<Unit, IErrorBuilder> EnsureNotReserved()
        {
            if (ReservedVariableNames.Contains(Name))
                return new ErrorBuilder($"The VariableName <{Name}> is Reserved.", ErrorCode.ReservedVariableName);
            if (Name.StartsWith(ReservedVariableNamePrefix, StringComparison.OrdinalIgnoreCase))
                return new ErrorBuilder($"The VariableName Prefix '{ReservedVariableNamePrefix}' is Reserved.", ErrorCode.ReservedVariableName);

            return Unit.Default;
        }


        /// <summary>
        /// The variable that entities will be set to.
        /// </summary>

        public static VariableName Entity { get; } = new VariableName("Entity");

        /// <summary>
        /// The variable that entities will be set to.
        /// </summary>

        public static VariableName Property { get; } = new VariableName("Property");

        /// <summary>
        /// Prefix reserved for internal use
        /// </summary>
        private const string ReservedVariableNamePrefix = "Reductech";

        private static readonly HashSet<string> ReservedVariableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Entity.Name,
            Property.Name
        };
    }
}