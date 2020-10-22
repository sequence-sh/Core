namespace Reductech.EDR.Core.Internal.Errors
{
    public class EntireSequenceLocation : IErrorLocation
    {
        private EntireSequenceLocation() {}

        public static IErrorLocation Instance { get; } = new EntireSequenceLocation();

        /// <inheritdoc />
        public string AsString => "Entire Sequence";
    }
}