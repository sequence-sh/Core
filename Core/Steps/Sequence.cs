using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
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
        public override async Task<Result<Unit, IRunErrors>>  Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var remainingSteps = new Stack<IStep<Unit>>(Steps.Reverse());

            while (remainingSteps.TryPop(out var step))
            {
                if (step.StepCombiners.Any() && remainingSteps.TryPop(out var nextStep))
                {
                    var combined = false;
                    foreach (var stepCombiner in step.StepCombiners)
                    {
                        var combineResult = stepCombiner.TryCombine(step, nextStep);
                        if (combineResult.IsSuccess)
                        {
                            remainingSteps.Push(combineResult.Value);
                            combined = true;
                            break;
                        }
                    }

                    if(!combined)
                        remainingSteps.Push(nextStep); //put it back
                    else
                        continue; //try combining the combined result

                }

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
            var fpd = new FreezableStepData(new Dictionary<string, StepMember>()
            {
                {nameof(Sequence.Steps), new StepMember(steps.ToList())}
            });

            return new CompoundFreezableStep(Instance, fpd, configuration);
        }
    }
}