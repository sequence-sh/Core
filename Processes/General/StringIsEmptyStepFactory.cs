using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Returns whether a string is empty.
    /// </summary>
    public sealed class StringIsEmptyStepFactory : SimpleStepFactory<StringIsEmpty, bool>
    {
        private StringIsEmptyStepFactory() { }

        public static SimpleStepFactory<StringIsEmpty, bool> Instance { get; } = new StringIsEmptyStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"'[{nameof(LengthOfString.String)}]' is empty?");
    }
}