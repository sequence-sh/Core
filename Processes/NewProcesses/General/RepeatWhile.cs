using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{

    /// <summary>
    /// Repeat an action while the condition is met.
    /// </summary>
    public sealed class RepeatWhile : CompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit> Run(ProcessState processState)
        {
            while (true)
            {
                var conditionResult = Condition.Run(processState);
                if (conditionResult.IsFailure) return conditionResult.ConvertFailure<Unit>();

                if (conditionResult.Value)
                {
                    var actionResult = Action.Run(processState);
                    if (actionResult.IsFailure) return actionResult.ConvertFailure<Unit>();
                }
                else break;
            }

            return Unit.Default;
        }

        /// <summary>
        /// The action to perform repeatedly.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<Unit> Action { get; set; }


        /// <summary>
        /// The condition to check before performing the action.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<bool> Condition { get; set; }

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => RepeatWhileProcessFactory.Instance;
    }

    /// <summary>
    /// Repeat an action while the condition is met.
    /// </summary>
    public sealed class RepeatWhileProcessFactory : SimpleRunnableProcessFactory<RepeatWhile, Unit>
    {
        private RepeatWhileProcessFactory() { }

        public static SimpleRunnableProcessFactory<RepeatWhile, Unit> Instance { get; } = new RepeatWhileProcessFactory();

        /// <inheritdoc />
        protected override string ProcessNameTemplate => $"Repeat '[{nameof(RepeatWhile.Action)}]' while '[{nameof(RepeatWhile.Condition)}]'";
    }
}