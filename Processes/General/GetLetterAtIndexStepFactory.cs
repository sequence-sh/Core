using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Gets the letters that appears at a specific index
    /// </summary>
    public sealed class GetLetterAtIndexStepFactory : SimpleStepFactory<GetLetterAtIndex, string>
    {
        private GetLetterAtIndexStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<GetLetterAtIndex, string> Instance { get; } = new GetLetterAtIndexStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Get character at index '[{nameof(GetLetterAtIndex.Index)}]' in '[{nameof(GetLetterAtIndex.String)}]'");
    }
}