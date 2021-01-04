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
    /// Do an action for each value of &lt;i&gt; in a range.
    /// </summary>
    public sealed class For : CompoundStep<Unit>
    {

        /// <summary>
        /// The first value of the variable to use.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<int> From { get; set; } = null!;

        /// <summary>
        /// The highest value of the variable to use
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<int> To { get; set; } = null!;


        /// <summary>
        /// The amount to increment by each iteration.
        /// </summary>
        [StepProperty(3)]
        [Required]
        public IStep<int> Increment { get; set; } = null!;

        /// <summary>
        /// The action to perform repeatedly.
        /// </summary>
        [StepProperty(4)]
        [ScopedFunction]
        [Required]
        public IStep<Unit> Action { get; set; } = null!;

        /// <summary>
        /// The name of the variable to use within the action.
        /// </summary>
        [VariableName(5)]
        [DefaultValueExplanation("<i>")]

        public VariableName Variable { get; set; } = VariableName.Index;

        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var variableName = VariableName.Index;

            var from = await From.Run(stateMonad, cancellationToken);
            if (from.IsFailure) return from.ConvertFailure<Unit>();

            var to = await To.Run(stateMonad, cancellationToken);
            if (to.IsFailure) return to.ConvertFailure<Unit>();

            var increment = await Increment.Run(stateMonad, cancellationToken);
            if (increment.IsFailure) return increment.ConvertFailure<Unit>();

            var currentValue = from.Value;

            var setResult = stateMonad.SetVariable(variableName, currentValue);
            if (setResult.IsFailure) return setResult.ConvertFailure<Unit>();

            if(increment.Value == 0)
                return new SingleError("Cannot do a For loop with an increment of 0", ErrorCode.DivideByZero, new StepErrorLocation(this));

            while (increment.Value > 0? currentValue <= to.Value : currentValue >= to.Value)
            {
                var r = await Action.Run(stateMonad, cancellationToken);
                if (r.IsFailure) return r;


                var currentValueResult = stateMonad.GetVariable<int>(variableName).MapError(e=>e.WithLocation(this));
                if (currentValueResult.IsFailure) return currentValueResult.ConvertFailure<Unit>();
                currentValue = currentValueResult.Value;
                currentValue += increment.Value;

                var setResult2 = stateMonad.SetVariable(variableName, currentValue);
                if (setResult2.IsFailure) return setResult.ConvertFailure<Unit>();
            }

            stateMonad.RemoveVariable(VariableName.Index, false);

            return Unit.Default;

        }

        /// <inheritdoc />
        public override Result<StepContext, IError> TryGetScopedContext(StepContext baseContext)
        {
            return baseContext.TryClone((Variable, new ActualTypeReference(typeof(int))))
                .MapError(eb => eb.WithLocation(this));
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ForStepFactory.Instance;
    }

    /// <summary>
    /// Do an action for each value of a given variable in a range.
    /// </summary>
    public class ForStepFactory : SimpleStepFactory<For, Unit>
    {
        private ForStepFactory() { }


        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<For, Unit> Instance { get; } = new ForStepFactory();
    }
}