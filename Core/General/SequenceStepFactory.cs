using System.Collections.Generic;
using System.Linq;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
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
        public static IFreezableStep CreateFreezable(IEnumerable<IFreezableStep> steps, Configuration? configuration)
        {
            var fpd = new FreezableStepData(new Dictionary<string, StepMember>()
            {
                {nameof(Sequence.Steps), new StepMember(steps.ToList())}
            });

            return new CompoundFreezableStep(Instance, fpd, configuration);
        }
    }
}