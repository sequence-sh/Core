using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Do an action for each value of a given variable in a range.
    /// </summary>
    public sealed class For : CompoundStep<Unit>
    {

        /// <summary>
        /// The action to perform repeatedly.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<Unit> Action { get; set; } = null!;


        /// <summary>
        /// The name of the variable to loop over.
        /// </summary>
        [VariableName]
        [Required]
        public VariableName VariableName { get; set; }

        /// <summary>
        /// The first value of the variable to use.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<int> From { get; set; } = null!;

        /// <summary>
        /// The highest value of the variable to use
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<int> To { get; set; } = null!;


        /// <summary>
        /// The amount to increment by each iteration.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<int> Increment { get; set; } = null!;

        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad)
        {
            var from = From.Run(stateMonad);
            if (from.IsFailure) return from.ConvertFailure<Unit>();

            var to = To.Run(stateMonad);
            if (to.IsFailure) return from.ConvertFailure<Unit>();

            var increment = Increment.Run(stateMonad);
            if (increment.IsFailure) return from.ConvertFailure<Unit>();

            var currentValue = from.Value;

            var setResult = stateMonad.SetVariable(VariableName, currentValue);
            if (setResult.IsFailure) return setResult.ConvertFailure<Unit>();

            while (currentValue <= to.Value)
            {
                var r = Action.Run(stateMonad);
                if (r.IsFailure) return r;


                var currentValueResult = stateMonad.GetVariable<int>(VariableName, Name);
                if (currentValueResult.IsFailure) return currentValueResult.ConvertFailure<Unit>();
                currentValue = currentValueResult.Value;
                currentValue += increment.Value;

                var setResult2 = stateMonad.SetVariable(VariableName, currentValue);
                if (setResult2.IsFailure) return setResult.ConvertFailure<Unit>();
            }

            return Unit.Default;

        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ForStepFactory.Instance;
    }
}