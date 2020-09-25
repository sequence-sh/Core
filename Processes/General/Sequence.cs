using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Serialization;
using Reductech.EDR.Processes.Util;

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
            var remainingSteps = new Stack<IRunnableProcess<Unit>>(Steps.Reverse());

            while (remainingSteps.TryPop(out var runnableProcess))
            {
                if (runnableProcess.ProcessCombiners.Any() && remainingSteps.TryPop(out var nextProcess))
                {
                    var combined = false;
                    foreach (var processCombiner in runnableProcess.ProcessCombiners)
                    {
                        var combineResult = processCombiner.TryCombine(runnableProcess, nextProcess);
                        if (combineResult.IsSuccess)
                        {
                            remainingSteps.Push(combineResult.Value);
                            combined = true;
                            break;
                        }
                    }

                    if(!combined)
                        remainingSteps.Push(nextProcess); //put it back
                    else
                        continue; //try combining the combined result

                }

                var r = runnableProcess.Run(processState);
                if (r.IsFailure)
                    return r.ConvertFailure<Unit>();

            }

            return Unit.Default;
        }

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => SequenceProcessFactory.Instance;

        /// <summary>
        /// The steps of this sequence.
        /// </summary>
        [RunnableProcessListPropertyAttribute]
        [Required]
        public IReadOnlyList<IRunnableProcess<Unit>> Steps { get; set; } = null!;
    }

    /// <summary>
    /// A sequence of steps to be run one after the other.
    /// </summary>
    public sealed class SequenceProcessFactory : SimpleRunnableProcessFactory<Sequence, Unit>
    {
        private SequenceProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RunnableProcessFactory Instance { get; } = new SequenceProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"[{nameof(Sequence.Steps)}]");


        /// <inheritdoc />
        public override IProcessSerializer Serializer => NoSpecialSerializer.Instance;

        /// <summary>
        /// Create a new Freezable Sequence
        /// </summary>
        public static IFreezableProcess CreateFreezable(IEnumerable<IFreezableProcess> processes, ProcessConfiguration? configuration)
        {
            var fpd = new FreezableProcessData(new Dictionary<string, ProcessMember>()
            {
                {nameof(Sequence.Steps), new ProcessMember(processes.ToList())}
            });

            return new CompoundFreezableProcess(Instance, fpd, configuration);
        }
    }
}