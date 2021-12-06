namespace Reductech.EDR.Core
{

/// <summary>
/// The Null value in SCL
/// </summary>
public record SCLNull
{
    private SCLNull() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static SCLNull Instance { get; } = new();
}

}
