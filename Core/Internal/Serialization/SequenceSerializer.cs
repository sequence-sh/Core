using System.Text;
using Reductech.Sequence.Core.Steps;

namespace Reductech.Sequence.Core.Internal.Serialization;

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
    public string Serialize(SerializeOptions options, IEnumerable<StepProperty> stepProperties)
    {
        var dict = stepProperties.ToDictionary(x => x.Name);

        var sb = new StringBuilder();

        if (dict.TryGetValue(nameof(Sequence<ISCLObject>.InitialSteps), out var sp)
         && sp is StepProperty.StepListProperty stepList)
            foreach (var step in stepList.StepList)
                AddStep(options, sb, step, 0);

        if (dict.TryGetValue(nameof(Sequence<ISCLObject>.FinalStep), out var finalStep)
         && finalStep is StepProperty.SingleStepProperty stepProperty)
            AddStep(options, sb, stepProperty.Step, 0);

        var s = sb.ToString();

        return s;

        static void AddStep(
            SerializeOptions options,
            StringBuilder sb,
            IStep step,
            int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel);

            if (step is ISequenceStep sequence)
            {
                sb.AppendLine($"{indentation}- (");

                foreach (var nestedStep in sequence.AllSteps)
                {
                    AddStep(options, sb, nestedStep, nestingLevel + 1);
                }

                sb.AppendLine($"{indentation})");
            }
            else
            {
                sb.AppendLine($"{indentation}- {step.Serialize(options)}");
            }
        }
    }
}
