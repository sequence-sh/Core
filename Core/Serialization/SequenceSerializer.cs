using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public async Task<string> SerializeAsync(IEnumerable<StepProperty> stepProperties, CancellationToken cancellationToken)
        {
            var dict = stepProperties.ToDictionary(x=>x.Name);

            var sb = new StringBuilder();


            if (dict.TryGetValue(nameof(Sequence<object>.InitialSteps), out var stepList) && stepList.Value.IsT2)
                foreach (var step in stepList.Value.AsT2)
                    sb.AppendLine("- " + await step.SerializeAsync(cancellationToken));

            if(dict.TryGetValue(nameof(Sequence<object>.FinalStep), out var finalStep) && finalStep.Value.IsT1)
                sb.AppendLine("- " + await finalStep.Value.AsT1.SerializeAsync(cancellationToken));

            var s = sb.ToString();

            return s;
        }
    }
}