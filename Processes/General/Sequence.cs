using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
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

            while (remainingSteps.TryPop(out var runnableProcess))
            {
                if (runnableProcess.StepCombiners.Any() && remainingSteps.TryPop(out var nextProcess))
                {
                    var combined = false;
                    foreach (var processCombiner in runnableProcess.StepCombiners)
                    {
                        var combineResult = processCombiner.TryCombine(runnableProcess, nextProcess);
                        if (combineResult.IsSuccess)
                        {
                            remainingSteps.Push(combineResult.Value);
                            combined = true;
                            break;
                        }
                    }

                    if(!combined)
                        remainingSteps.Push(nextProcess); //put it back
                    else
                        continue; //try combining the combined result

                }

                var r = runnableProcess.Run(stateMonad);
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