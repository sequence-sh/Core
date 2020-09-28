using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Repeat an action while the condition is met.
    /// </summary>
    public sealed class RepeatWhileStepFactory : SimpleStepFactory<RepeatWhile, Unit>
    {
        private RepeatWhileStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<RepeatWhile, Unit> Instance { get; } = new RepeatWhileStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Repeat '[{nameof(RepeatWhile.Action)}]' while '[{nameof(RepeatWhile.Condition)}]'");
    }
}