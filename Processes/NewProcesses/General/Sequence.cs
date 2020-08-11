using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// A sequence of steps to be run one after the other.
    /// </summary>
    public sealed class SequenceProcessFactory : SimpleRunnableProcessFactory<SequenceProcessFactory.Sequence, Unit>
    {
        private SequenceProcessFactory() { }

        public static RunnableProcessFactory Instance { get; } = new SequenceProcessFactory();

        /// <inheritdoc />
        protected override string ProcessNameTemplate => $"[{nameof(Sequence.Steps)}]";


        /// <summary>
        /// A sequence of steps to be run one after the other.
        /// </summary>
        public sealed class Sequence : CompoundRunnableProcess<Unit>
        {
            /// <inheritdoc />
            public override Result<Unit> Run(ProcessState processState)
            {
                foreach (var runnableProcess in Steps)
                {
                    var r = runnableProcess.Run(processState);
                    if (r.IsFailure)
                        return r.ConvertFailure<Unit>();
                }

                return Unit.Default;
            }

            /// <inheritdoc />
            public override RunnableProcessFactory RunnableProcessFactory => SequenceProcessFactory.Instance;

            /// <summary>
            /// The steps of this sequence.
            /// </summary>
            [RunnableProcessListProperty]
            [Required]
            public IReadOnlyList<IRunnableProcess<Unit>> Steps { get; set; } = null!;
        }
    }
}