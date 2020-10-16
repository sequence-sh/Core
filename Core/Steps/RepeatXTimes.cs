using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Repeat a step a set number of times.
    /// </summary>
    public sealed class RepeatXTimes : CompoundStep<Unit>
    {
        /// <summary>
        /// The action to perform repeatedly.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<Unit> Action { get; set; } = null!;

        /// <summary>
        /// The number of times to perform the action.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<int> Number { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<Unit, IRunErrors>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var numberResult = await Number.Run(stateMonad, cancellationToken);

            if (numberResult.IsFailure) return numberResult.ConvertFailure<Unit>();

            for (var i = 0; i < numberResult.Value; i++)
            {
                var result = await Action.Run(stateMonad, cancellationToken);
                if (result.IsFailure) return result.ConvertFailure<Unit>();
            }

            return Unit.Default;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => RepeatXTimesStepFactory.Instance;
    }

    /// <summary>
    /// Repeat a step a set number of times.
    /// </summary>
    public sealed class RepeatXTimesStepFactory : SimpleStepFactory<RepeatXTimes, Unit>
    {
        private RepeatXTimesStepFactory() { }
        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<RepeatXTimes, Unit> Instance { get; } = new RepeatXTimesStepFactory();


        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Repeat '[{nameof(RepeatXTimes.Action)}]' '[{nameof(RepeatXTimes.Number)}]' times.");
    }
}