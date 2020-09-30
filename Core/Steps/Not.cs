using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;

namespace Reductech.EDR.Core.Steps
{

    /// <summary>
    /// Negation of a boolean value.
    /// </summary>
    public sealed class Not : CompoundStep<bool>
    {
        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(StateMonad stateMonad) => Boolean.Run(stateMonad).Map(x => !x);

        /// <inheritdoc />
        public override IStepFactory StepFactory => NotStepFactory.Instance;

        /// <summary>
        /// The value to negate.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<bool> Boolean { get; set; } = null!;
    }

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