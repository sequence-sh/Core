using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Do an action for each value of a given variable in a range.
    /// </summary>
    public sealed class For : CompoundRunnableProcess<Unit>
    {

        /// <summary>
        /// The action to perform repeatedly.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<Unit> Action { get; set; } = null!;


        /// <summary>
        /// The name of the variable to loop over.
        /// </summary>
        [VariableName]
        [Required]
        public VariableName VariableName { get; set; }

        /// <summary>
        /// The first value of the variable to use.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<int> From { get; set; } = null!;

        /// <summary>
        /// The highest value of the variable to use
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<int> To { get; set; } = null!;


        /// <summary>
        /// The amount to increment by each iteration.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<int> Increment { get; set; } = null!;

        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(ProcessState processState)
        {
            var from = From.Run(processState);
            if (from.IsFailure) return from.ConvertFailure<Unit>();

            var to = To.Run(processState);
            if (to.IsFailure) return from.ConvertFailure<Unit>();

            var increment = Increment.Run(processState);
            if (increment.IsFailure) return from.ConvertFailure<Unit>();

            var currentValue = from.Value;

            var setResult = processState.SetVariable(VariableName, currentValue);
            if (setResult.IsFailure) return setResult.ConvertFailure<Unit>();

            while (currentValue <= to.Value)
            {
                var r = Action.Run(processState);
                if (r.IsFailure) return r;


                var currentValueResult = processState.GetVariable<int>(VariableName, Name);
                if (currentValueResult.IsFailure) return currentValueResult.ConvertFailure<Unit>();
                currentValue = currentValueResult.Value;
                currentValue += increment.Value;

                var setResult2 = processState.SetVariable(VariableName, currentValue);
                if (setResult2.IsFailure) return setResult.ConvertFailure<Unit>();
            }

            return Unit.Default;

        }

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => ForProcessFactory.Instance;
    }

    /// <summary>
    /// Do an action for each value of a given variable in a range.
    /// </summary>
    public class ForProcessFactory : SimpleRunnableProcessFactory<For, Unit>
    {
        private ForProcessFactory() { }


        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleRunnableProcessFactory<For, Unit> Instance { get; } = new ForProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"For [{nameof(For.VariableName)}] = [{nameof(For.From)}]; [{nameof(For.VariableName)}] <= [{nameof(For.To)}]; += [{nameof(For.Increment)}]; [{nameof(For.Action)}]");


        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName, FreezableProcessData freezableProcessData) => Maybe<ITypeReference>.From(new ActualTypeReference(typeof(int)));
    }
}