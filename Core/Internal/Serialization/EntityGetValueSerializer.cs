namespace Reductech.Sequence.Core.Internal.Serialization;

/// <summary>
/// Serializer for EntityGetValue
/// </summary>
public class EntityGetValueSerializer : IStepSerializer
{
    private EntityGetValueSerializer() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static IStepSerializer Instance { get; } = new EntityGetValueSerializer();

    /// <inheritdoc />
    public string Serialize(SerializeOptions options, IEnumerable<StepProperty> stepProperties)
    {
        var (first, second) = stepProperties.GetFirstTwo().GetValueOrThrow();

        var entity = first.Serialize(options);
        var index  = second.Serialize(options);

        return $"{entity}[{index}]";
    }

    /// <inheritdoc />
    public void Format(
        IEnumerable<StepProperty> stepProperties,
        TextLocation? textLocation,
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments)
    {
        var (first, second) = stepProperties.GetFirstTwo().GetValueOrThrow();

        indentationStringBuilder.AppendPrecedingComments(remainingComments, textLocation);

        first.Format(indentationStringBuilder, options, remainingComments);
        indentationStringBuilder.Append("[");
        second.Format(indentationStringBuilder, options, remainingComments);
        indentationStringBuilder.Append("]");
    }
}
