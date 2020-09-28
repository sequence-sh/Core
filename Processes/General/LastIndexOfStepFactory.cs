using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Gets the last instance of substring in a string.
    /// </summary>
    public sealed class LastIndexOfStepFactory : SimpleStepFactory<LastIndexOf, int>
    {
        private LastIndexOfStepFactory() { }

        public static SimpleStepFactory<LastIndexOf, int> Instance { get; } = new LastIndexOfStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Last index of '[{nameof(LastIndexOf.SubString)}]' in '[{nameof(LastIndexOf.String)}]'");
    }
}