using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Join strings with a delimiter.
    /// </summary>
    public sealed class JoinStringsStepFactory : SimpleStepFactory<JoinStrings, string>
    {
        private JoinStringsStepFactory() { }

        public static SimpleStepFactory<JoinStrings, string> Instance { get; } = new JoinStringsStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Join [{nameof(JoinStrings.List)}]");
    }
}