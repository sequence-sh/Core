using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reductech.EDR.Core.Steps;

namespace Reductech.EDR.Core.Internal.Serialization
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
        var dict = stepProperties.ToDictionary(x => x.Name);

        var sb = new StringBuilder();

        if (dict.TryGetValue(nameof(Sequence<object>.InitialSteps), out var sp)
         && sp is StepProperty.StepListProperty stepList)
            foreach (var step in stepList.StepList)
                sb.AppendLine("- " + step.Serialize());

        if (dict.TryGetValue(nameof(Sequence<object>.FinalStep), out var finalStep)
         && finalStep is StepProperty.SingleStepProperty stepProperty)
            sb.AppendLine("- " + stepProperty.Serialize());

        var s = sb.ToString();

        return s;
    }
}

}
