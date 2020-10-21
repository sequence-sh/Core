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
        public override async Task<Result<Unit, IError>>  Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var from = await From.Run(stateMonad, cancellationToken);
            if (from.IsFailure) return from.ConvertFailure<Unit>();

            var to = await To.Run(stateMonad, cancellationToken);
            if (to.IsFailure) return from.ConvertFailure<Unit>();

            var increment = await Increment.Run(stateMonad, cancellationToken);
            if (increment.IsFailure) return from.ConvertFailure<Unit>();

            var currentValue = from.Value;

            var setResult = stateMonad.SetVariable(VariableName, currentValue);
            if (setResult.IsFailure) return setResult.ConvertFailure<Unit>();

            while (currentValue <= to.Value)
            {
                var r = await Action.Run(stateMonad, cancellationToken);
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

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"For [{nameof(For.VariableName)}] = [{nameof(For.From)}]; [{nameof(For.VariableName)}] <= [{nameof(For.To)}]; += [{nameof(For.Increment)}]; [{nameof(For.Action)}]");


        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName,
            FreezableStepData freezableStepData, TypeResolver typeResolver) => Maybe<ITypeReference>.From(new ActualTypeReference(typeof(int)));
    }
}