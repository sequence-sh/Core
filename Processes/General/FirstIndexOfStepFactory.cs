using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Gets the first instance of substring in a string.
    /// </summary>
    public sealed class FirstIndexOfStepFactory : SimpleStepFactory<FirstIndexOf, int>
    {
        private FirstIndexOfStepFactory() { }

        public static SimpleStepFactory<FirstIndexOf, int> Instance { get; } = new FirstIndexOfStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"First index of '[{nameof(FirstIndexOf.SubString)}]' in '[{nameof(FirstIndexOf.String)}]'");
    }
}