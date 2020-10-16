using System.Collections.Immutable;
using Superpower.Model;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// The error returned by a failed Step Member parsing
    /// </summary>
    public class StepMemberParseError
    {
        /// <summary>
        /// Create a new StepMemberParseError
        /// </summary>
        public StepMemberParseError(string? errorMessage, Position errorPosition, ImmutableHashSet<string> expectations)
        {
            ErrorMessage = errorMessage;
            ErrorPosition = errorPosition;
            Expectations = expectations;
        }

        /// <summary>
        /// The error message, if there is one.
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// The position of the error.
        /// </summary>
        public Position ErrorPosition { get; }

        /// <summary>
        /// The expectations.
        /// </summary>
        public ImmutableHashSet<string> Expectations { get; }

        /// <inheritdoc />
        public override string ToString() => ErrorMessage ?? $"Parsing error at {ErrorPosition}";
    }
}