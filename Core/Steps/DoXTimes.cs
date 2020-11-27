using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Repeat a step a set number of times.
    /// </summary>
    public sealed class DoXTimes : CompoundStep<Unit>
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
        public IStep<int> X { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var numberResult = await X.Run(stateMonad, cancellationToken);

            if (numberResult.IsFailure) return numberResult.ConvertFailure<Unit>();

            for (var i = 0; i < numberResult.Value; i++)
            {
                var result = await Action.Run(stateMonad, cancellationToken);
                if (result.IsFailure) return result.ConvertFailure<Unit>();
            }

            return Unit.Default;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => DoXTimesStepFactory.Instance;
    }

    /// <summary>
    /// Repeat a step a set number of times.
    /// </summary>
    public sealed class DoXTimesStepFactory : SimpleStepFactory<DoXTimes, Unit>
    {
        private DoXTimesStepFactory() { }
        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<DoXTimes, Unit> Instance { get; } = new DoXTimesStepFactory();


        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Repeat '[{nameof(DoXTimes.Action)}]' '[{nameof(DoXTimes.X)}]' times.");
    }
}