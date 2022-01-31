using System.Text;

namespace Reductech.Sequence.Core.Internal.Serialization;

/// <summary>
/// The default step serializer for functions.
/// Produces results like: Print Value: 'Hello World'
/// </summary>
public sealed class FunctionSerializer : IStepSerializer
{
    /// <summary>
    /// Creates a new FunctionSerializer
    /// </summary>
    /// <param name="name"></param>
    public FunctionSerializer(string name) { Name = name; }

    /// <summary>
    /// The name of the function.
    /// </summary>
    public string Name { get; }

    /// <inheritdoc />
    public string Serialize(SerializeOptions options, IEnumerable<StepProperty> stepProperties)
    {
        var sb = new StringBuilder();
        sb.Append(Name);

        foreach (var stepProperty in stepProperties.OrderBy(x => x.Index))
        {
            sb.Append(' ');

            sb.Append(stepProperty.Name);
            sb.Append(": ");

            var value = stepProperty.Serialize(options);

            sb.Append(value);
        }

        return sb.ToString();
    }

    /// <inheritdoc />
    public void Format(
        IEnumerable<StepProperty> sps,
        TextLocation? textLocation,
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments)
    {
        indentationStringBuilder.Append(Name);
        var stepProperties = sps.OrderBy(x => x.Index).ToList();

        if (stepProperties.Count == 0)
        {
            return;
        }

        if (stepProperties.Count == 1)
        {
            var stepProperty = stepProperties.Single();

            indentationStringBuilder.AppendPrecedingComments(
                remainingComments,
                stepProperty.MaybeTextLocation()
            );

            indentationStringBuilder.Append($" {stepProperty.Name}: ");
            stepProperty.Format(indentationStringBuilder, options, remainingComments);
            return;
        }

        var maxNameLength = stepProperties.Select(x => x.Name.Length).Max();

        indentationStringBuilder.AppendJoin(
            " ",
            false,
            stepProperties,
            stepProperty =>
            {
                indentationStringBuilder.AppendPrecedingComments(
                    remainingComments,
                    stepProperty.MaybeTextLocation()
                );

                if (stepProperties.Count > 1)
                    indentationStringBuilder.AppendLineMaybe();

                indentationStringBuilder.Append($"{stepProperty.Name.PadRight(maxNameLength)}: ");
                stepProperty.Format(indentationStringBuilder, options, remainingComments);
            }
        );
    }
}
