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
    }
}