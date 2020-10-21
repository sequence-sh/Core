using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Serialization;

namespace Reductech.EDR.Core.Steps
{

    /// <summary>
    /// Negation of a boolean value.
    /// </summary>
    public sealed class Not : CompoundStep<bool>
    {
        /// <inheritdoc />
        public override async Task<Result<bool, IError>>  Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            return await Boolean.Run(stateMonad, cancellationToken).Map(x => !x);
        }

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


        /// <summary>
        /// Create a freezable Not step.
        /// </summary>
        public static IFreezableStep CreateFreezable(IFreezableStep boolean)
        {
            var dict = new Dictionary<string, IFreezableStep>
            {
                {nameof(Not.Boolean), boolean},
            };

            var fpd = new FreezableStepData(dict, null, null);
            var step = new CompoundFreezableStep(Instance, fpd, null);

            return step;
        }
    }
}