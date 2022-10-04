namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A reference to a variable
/// </summary>
public sealed record VariableReference(
    TypeReference TypeReference,
    bool Injected,
    string? Description,
    string? SerializedValue)
{
    /// <summary>
    /// Gets the description and value. Formatted
    /// </summary>
    public string? GetMarkdown()
    {
        if (Description is not null)
        {
            return SerializedValue is not null
                ? $"{Description}  \r\n*{SerializedValue}*"
                : Description;
        }

        return SerializedValue is not null ? $"*{SerializedValue}*" : null;
    }
}
