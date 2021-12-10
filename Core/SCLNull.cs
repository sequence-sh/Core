namespace Reductech.EDR.Core;

/// <summary>
/// The Null value in SCL
/// </summary>
public record SCLNull : ISCLObject
{
    private SCLNull() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static SCLNull Instance { get; } = new();

    /// <inheritdoc />
    public string Name => "Null";

    /// <inheritdoc />
    public string Serialize() => Name;

    /// <inheritdoc />
    public TypeReference TypeReference => TypeReference.Actual.Null;
}
