namespace Reductech.EDR.Core.Internal.Errors
{
    /// <summary>
    /// The error location was the entire sequence.
    /// </summary>
    public class EntireSequenceLocation : IErrorLocation
    {
        private EntireSequenceLocation() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static IErrorLocation Instance { get; } = new EntireSequenceLocation();

        /// <inheritdoc />
        public string AsString => "Entire Sequence";

        /// <inheritdoc />
        public bool Equals(IErrorLocation? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return other is EntireSequenceLocation;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IErrorLocation errorLocation && Equals(errorLocation);
        }

        /// <inheritdoc />
        public override int GetHashCode() => 42;
    }
}