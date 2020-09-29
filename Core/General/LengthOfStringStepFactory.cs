using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Calculates the length of the string.
    /// </summary>
    public sealed class LengthOfStringStepFactory : SimpleStepFactory<LengthOfString, int>
    {
        private LengthOfStringStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<LengthOfString, int> Instance { get; } = new LengthOfStringStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Length of [{nameof(LengthOfString.String)}]");

    }
}