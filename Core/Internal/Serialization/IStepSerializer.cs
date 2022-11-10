namespace Sequence.Core.Internal.Serialization;

/// <summary>
/// A custom step serializer.
/// </summary>
public interface IStepSerializer
{
    /// <summary>
    /// Serialize a step according to it's properties.
    /// </summary>
    string Serialize(SerializeOptions options, IEnumerable<StepProperty> stepProperties);

    /// <summary>
    /// Format a step according to it's properties
    /// </summary>
    void Format(
        IEnumerable<StepProperty> stepProperties,
        TextLocation? textLocation,
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments);
}
