namespace Sequence.Core.Internal;

/// <summary>
/// Options for serializing an SCL object
/// </summary>
/// <param name="QuoteStrings">Whether to enclose strings in quotes</param>
/// <param name="HideStrings">Whether to hide the content of strings</param>
/// <param name="MaxArrayLength">The maximum length of array to return</param>
/// <param name="EvaluateStreams">Whether to evaluate lazy arrays and StringStreams</param>
public sealed record SerializeOptions(
    bool QuoteStrings,
    bool HideStrings,
    bool EvaluateStreams,
    int? MaxArrayLength)
{
    /// <summary>
    /// Suitable for testing
    /// </summary>
    public static readonly SerializeOptions Name = new(false, false, false, 5);

    /// <summary>
    /// Suitable for logging
    /// </summary>
    public static readonly SerializeOptions SanitizedName = new(false, true, false, 5);

    /// <summary>
    /// Suitable for rendering SCL
    /// </summary>
    public static readonly SerializeOptions Serialize = new(true, false, true, null);

    /// <summary>
    /// Suitable for parsing values
    /// </summary>
    public static readonly SerializeOptions Primitive = new(false, false, true, null);
}
