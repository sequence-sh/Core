namespace Sequence.Core.Internal.Serialization;

/// <summary>
/// Serializes an array
/// </summary>
public sealed class ArraySerializer : IStepSerializer
{
    private ArraySerializer() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static IStepSerializer Instance { get; } = new ArraySerializer();

    /// <inheritdoc />
    public string Serialize(SerializeOptions options, IEnumerable<StepProperty> stepProperties) =>
        stepProperties.Single().Serialize(options);

    /// <inheritdoc />
    public void Format(
        IEnumerable<StepProperty> stepProperties,
        TextLocation? textLocation,
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments)
    {
        stepProperties.Single().Format(indentationStringBuilder, options, remainingComments);
    }
}
