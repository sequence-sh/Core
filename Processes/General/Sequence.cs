using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// A sequence of steps to be run one after the other.
    /// </summary>
    public sealed class Sequence : CompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(ProcessState processState)
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

    /// <summary>
    /// A sequence of steps to be run one after the other.
    /// </summary>
    public sealed class SequenceProcessFactory : SimpleRunnableProcessFactory<Sequence, Unit>
    {
        private SequenceProcessFactory() { }

        public static RunnableProcessFactory Instance { get; } = new SequenceProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"[{nameof(Sequence.Steps)}]");

    }
}