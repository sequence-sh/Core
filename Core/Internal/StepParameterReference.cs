using System;
using System.Collections.Generic;
using System.Reflection;
using OneOf;
using Reductech.EDR.Core.Attributes;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A reference to a step property.
    /// Either the name of the property or the argument order
    /// </summary>
    public readonly struct StepParameterReference :IEquatable<StepParameterReference>
    {
        /// <summary>
        /// Create a new StepParameterReference
        /// </summary>
        public StepParameterReference(OneOf<string, int> value) => Value = value;

        /// <summary>
        /// Either the name of the property or the argument order
        /// </summary>
        public OneOf<string, int> Value { get; }

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <summary>
        /// This reference, in human readable form
        /// </summary>
        public string Name => Value.Match(x => x, x => $"Parameter {x}");

        /// <inheritdoc />
        public bool Equals(StepParameterReference other)
        {
            if (Value.IsT0 && other.Value.IsT0)
                return Value.AsT0.Equals(other.Value.AsT0, StringComparison.OrdinalIgnoreCase);
            if (Value.IsT1 && other.Value.IsT1)
                return Value.AsT1.Equals(other.Value.AsT1);

            return false;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is StepParameterReference other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode() => Value.Match(x => StringComparer.OrdinalIgnoreCase.GetHashCode(x), x => x);

        /// <summary>
        /// Equals operator
        /// </summary>
        public static bool operator ==(StepParameterReference left, StepParameterReference right) => left.Equals(right);

        /// <summary>
        /// Not Equals operator
        /// </summary>
        public static bool operator !=(StepParameterReference left, StepParameterReference right) => !left.Equals(right);


        /// <summary>
        /// Gets possible StepParameterReferences for this property.
        /// </summary>
        public static IEnumerable<StepParameterReference> GetPossibleReferences(MemberInfo propertyInfo)
        {
            var attribute = propertyInfo.GetCustomAttribute<StepPropertyBaseAttribute>();

            if (attribute == null) yield break;

            yield return new StepParameterReference(propertyInfo.Name);
            yield return new StepParameterReference(attribute.Order);
        }
    }
}