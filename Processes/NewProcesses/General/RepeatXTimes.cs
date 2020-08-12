using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
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
        public IRunnableProcess<Unit> Action { get; set; }

        /// <summary>
        /// The number of times to perform the action.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<int> Number { get; set; }

        /// <inheritdoc />
        public override Result<Unit> Run(ProcessState processState)
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
        public override RunnableProcessFactory RunnableProcessFactory => RepeatXTimesProcessFactory.Instance;
    }

    /// <summary>
    /// Repeat a process a set number of times.
    /// </summary>
    public sealed class RepeatXTimesProcessFactory : SimpleRunnableProcessFactory<RepeatXTimes, Unit>
    {
        private RepeatXTimesProcessFactory() { }

        public static SimpleRunnableProcessFactory<RepeatXTimes, Unit> Instance { get; } = new RepeatXTimesProcessFactory();

        /// <inheritdoc />
        protected override string ProcessNameTemplate => $"Repeat '[{nameof(RepeatXTimes.Action)}]' '[{nameof(RepeatXTimes.Number)}]' times.";
    }
}