using System.Collections.Generic;
using System.Linq;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Serialization;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// A sequence of steps to be run one after the other.
    /// </summary>
    public sealed class SequenceStepFactory : SimpleStepFactory<Sequence, Unit>
    {
        private SequenceStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static StepFactory Instance { get; } = new SequenceStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"[{nameof(Sequence.Steps)}]");


        /// <inheritdoc />
        public override IStepSerializer Serializer => NoSpecialSerializer.Instance;

        /// <summary>
        /// Create a new Freezable Sequence
        /// </summary>
        public static IFreezableStep CreateFreezable(IEnumerable<IFreezableStep> processes, Configuration? configuration)
        {
            var fpd = new FreezableStepData(new Dictionary<string, StepMember>()
            {
                {nameof(Sequence.Steps), new StepMember(processes.ToList())}
            });

            return new CompoundFreezableStep(Instance, fpd, configuration);
        }
    }
}