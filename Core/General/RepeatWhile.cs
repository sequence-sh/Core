using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{

    /// <summary>
    /// Repeat an action while the condition is met.
    /// </summary>
    public sealed class RepeatWhile : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad)
        {
            while (true)
            {
                var conditionResult = Condition.Run(stateMonad);
                if (conditionResult.IsFailure) return conditionResult.ConvertFailure<Unit>();

                if (conditionResult.Value)
                {
                    var actionResult = Action.Run(stateMonad);
                    if (actionResult.IsFailure) return actionResult.ConvertFailure<Unit>();
                }
                else break;
            }

            return Unit.Default;
        }

        /// <summary>
        /// The condition to check before performing the action.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<bool> Condition { get; set; } = null!;

        /// <summary>
        /// The action to perform repeatedly.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<Unit> Action { get; set; } = null!;




        /// <inheritdoc />
        public override IStepFactory StepFactory => RepeatWhileStepFactory.Instance;
    }
}