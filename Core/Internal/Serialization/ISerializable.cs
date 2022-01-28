namespace Reductech.Sequence.Core.Internal.Serialization;

/// <summary>
/// Indicates an object that can serialized and formatted
/// </summary>
public interface ISerializable
{
    /// <summary>
    /// Serialize this object
    /// </summary>
    [Pure]
    string Serialize(SerializeOptions options);

    /// <summary>
    /// Format this object as a multiline indented string
    /// </summary>
    public void Format(
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments);
}
