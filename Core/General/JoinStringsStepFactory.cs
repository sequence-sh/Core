using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Join strings with a delimiter.
    /// </summary>
    public sealed class JoinStringsStepFactory : SimpleStepFactory<JoinStrings, string>
    {
        private JoinStringsStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<JoinStrings, string> Instance { get; } = new JoinStringsStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Join [{nameof(JoinStrings.List)}]");
    }
}