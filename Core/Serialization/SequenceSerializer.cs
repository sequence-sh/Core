using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Serializes a sequence
    /// </summary>
    public sealed class SequenceSerializer : IStepSerializer
    {
        private SequenceSerializer() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static IStepSerializer Instance { get; } = new SequenceSerializer();

        /// <inheritdoc />
        public string Serialize(IEnumerable<StepProperty> stepProperties)
        {
            var dict = stepProperties.ToDictionary(x=>x.Name);

            var sb = new StringBuilder();


            if (dict.TryGetValue(nameof(Sequence<object>.InitialSteps), out var stepList) && stepList.IsT2)
                foreach (var step in stepList.AsT2)
                    sb.AppendLine("- " + step.Serialize());

            if(dict.TryGetValue(nameof(Sequence<object>.FinalStep), out var finalStep) && finalStep.IsT1)
                sb.AppendLine("- " + finalStep.AsT1.Serialize());

            var s = sb.ToString();

            return s;
        }
    }
}