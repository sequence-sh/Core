using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Repeat a process a set number of times.
    /// </summary>
    public sealed class RepeatXTimes : CompoundRunnableProcess<Unit>
    {
        /// <summary>
        /// The action to perform repeatedly.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<Unit> Action { get; set; } = null!;

        /// <summary>
        /// The number of times to perform the action.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<int> Number { get; set; } = null!;

        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(ProcessState processState)
        {
            var numberResult = Number.Run(processState);

            if (numberResult.IsFailure) return numberResult.ConvertFailure<Unit>();

            for (var i = 0; i < numberResult.Value; i++)
            {
                var result = Action.Run(processState);
                if (result.IsFailure) return result.ConvertFailure<Unit>();
            }

            return Unit.Default;
        }

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => RepeatXTimesProcessFactory.Instance;
    }

    /// <summary>
    /// Repeat a process a set number of times.
    /// </summary>
    public sealed class RepeatXTimesProcessFactory : SimpleRunnableProcessFactory<RepeatXTimes, Unit>
    {
        private RepeatXTimesProcessFactory() { }

        public static SimpleRunnableProcessFactory<RepeatXTimes, Unit> Instance { get; } = new RepeatXTimesProcessFactory();


        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"Repeat '[{nameof(RepeatXTimes.Action)}]' '[{nameof(RepeatXTimes.Number)}]' times.");
    }
}