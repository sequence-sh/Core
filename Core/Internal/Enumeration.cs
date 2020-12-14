using System;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A member of a set of predefined values
    /// </summary>
    public sealed class Enumeration : IEquatable<Enumeration>//TODO make this a record
    {
        /// <summary>
        /// Create a new Enumeration
        /// </summary>
        public Enumeration(string type, string value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// The enum type
        /// </summary>
        public string Type { get;  }

        /// <summary>
        /// The enum value
        /// </summary>
        public string Value { get;}

        /// <inheritdoc />
        public override string ToString() => Type + "." + Value;

        /// <summary>
        /// Try to convert this to a C# enum
        /// </summary>
        public Maybe<T> TryConvert<T>() where T : struct, Enum
        {
            if (Enum.TryParse(Value, true, out T t))
                return t;

            return Maybe<T>.None;
        }


        /// <inheritdoc />
        public bool Equals(Enumeration? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Type == other.Type && Value == other.Value;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Enumeration other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Type, Value);

        /// <summary>
        /// Equals operator
        /// </summary>
        public static bool operator ==(Enumeration? left, Enumeration? right) => Equals(left, right);

        /// <summary>
        /// Not equals operator
        /// </summary>
        public static bool operator !=(Enumeration? left, Enumeration? right) => !Equals(left, right);
    }
}