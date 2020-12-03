using System;

namespace Reductech.EDR.Core.Internal
{
    public sealed class Enumeration : IEquatable<Enumeration>
    {
        //TODO make a record
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