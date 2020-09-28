using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Serialization;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Negation of a boolean value.
    /// </summary>
    public class NotStepFactory : SimpleStepFactory<Not, bool>
    {
        private NotStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static StepFactory Instance { get; } = new NotStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Not [{nameof(Not.Boolean)}]");

        /// <inheritdoc />
        public override IStepSerializer Serializer { get; } = new StepSerializer(
            new FixedStringComponent("not"),
            new FixedStringComponent("("),
            new BooleanComponent(nameof(Not.Boolean)),
            new FixedStringComponent(")")
        );
    }
}