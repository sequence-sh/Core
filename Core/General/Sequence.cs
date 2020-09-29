using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// A sequence of steps to be run one after the other.
    /// </summary>
    public sealed class Sequence : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad)
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

                var r = step.Run(stateMonad);
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
}