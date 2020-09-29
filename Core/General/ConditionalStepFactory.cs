using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Executes a statement if a condition is true.
    /// </summary>
    public sealed class ConditionalStepFactory : SimpleStepFactory<Conditional, Unit>
    {
        private ConditionalStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static ConditionalStepFactory Instance { get; } = new ConditionalStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"If [{nameof(Conditional.Condition)}] then [{nameof(Conditional.ThenStep)}] else [{nameof(Conditional.ElseStep)}]");
    }
}