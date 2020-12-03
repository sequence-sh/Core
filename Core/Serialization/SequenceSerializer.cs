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


            if (dict.TryGetValue(nameof(Sequence<object>.Steps), out var stepList) && stepList.Value.IsT2)
                foreach (var step in stepList.Value.AsT2)
                    sb.AppendLine("- " + step.Serialize());

            if(dict.TryGetValue(nameof(Sequence<object>.FinalStep), out var finalStep) && finalStep.Value.IsT1)
                sb.AppendLine("- " + finalStep.Value.AsT1.Serialize());

            var s = sb.ToString();

            return s;
        }
    }
}