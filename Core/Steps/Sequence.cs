using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// A sequence of steps to be run one after the other.
    /// </summary>
    public sealed class Sequence : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var remainingSteps = new Stack<IStep<Unit>>(Steps.Reverse());

            while (remainingSteps.TryPop(out var step))
            {
                var r = await step.Run(stateMonad, cancellationToken);
                if (r.IsFailure)
                    return r.ConvertFailure<Unit>();

            }

            return Unit.Default;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => SequenceStepFactory.Instance;

        /// <summary>
        /// The steps of this sequence.
        /// </summary>
        [StepListProperty]
        [Required]
        public IReadOnlyList<IStep<Unit>> Steps { get; set; } = null!;
    }

    /// <summary>
    /// A sequence of steps to be run one after the other.
    /// </summary>
    public sealed class SequenceStepFactory : SimpleStepFactory<Sequence, Unit>
    {
        private SequenceStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static StepFactory Instance { get; } = new SequenceStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"[{nameof(Sequence.Steps)}]");


        /// <inheritdoc />
        public override IStepSerializer Serializer => NoSpecialSerializer.Instance;

        /// <summary>
        /// Create a new Freezable Sequence
        /// </summary>
        public static IFreezableStep CreateFreezable(IEnumerable<IFreezableStep> steps, Configuration? configuration)
        {
            var dict = new Dictionary<string, IReadOnlyList<IFreezableStep>>()
            {
                {nameof(Sequence.Steps), steps.ToList()}
            };

            var fpd = new FreezableStepData(null, null, dict);


            return new CompoundFreezableStep(Instance, fpd, configuration);
        }
    }
}