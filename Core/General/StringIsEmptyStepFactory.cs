using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
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