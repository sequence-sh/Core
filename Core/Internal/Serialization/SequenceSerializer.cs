using System.Text;
using Sequence.Core.Steps;

namespace Sequence.Core.Internal.Serialization;

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

    private IEnumerable<IStep> GetAllSteps(IEnumerable<StepProperty> stepProperties)
    {
        var dict = stepProperties.ToDictionary(x => x.Name);

        if (dict.TryGetValue(nameof(Sequence<ISCLObject>.InitialSteps), out var sp)
         && sp is StepProperty.StepListProperty stepList)
            foreach (var step in stepList.StepList)
                yield return step;

        if (dict.TryGetValue(nameof(Sequence<ISCLObject>.FinalStep), out var finalStep)
         && finalStep is StepProperty.SingleStepProperty stepProperty)
            yield return stepProperty.Step;
    }

    /// <inheritdoc />
    public string Serialize(SerializeOptions options, IEnumerable<StepProperty> stepProperties)
    {
        var sb    = new StringBuilder();
        var steps = GetAllSteps(stepProperties);

        foreach (var step in steps)
            AddStep(options, sb, step, 0);

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

    /// <inheritdoc />
    public void Format(
        IEnumerable<StepProperty> stepProperties,
        TextLocation? textLocation,
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments)
    {
        foreach (var step in GetAllSteps(stepProperties))
        {
            indentationStringBuilder.AppendLine();
            indentationStringBuilder.AppendPrecedingComments(remainingComments, step.TextLocation);
            indentationStringBuilder.Append("- ");
            step.Format(indentationStringBuilder, options, remainingComments);
        }
    }
}
