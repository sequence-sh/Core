namespace Reductech.EDR.Core.Util;

/// <summary>
/// The result of a step with not return value.
/// </summary>
public sealed record Unit : ISCLObject
{
    /// <summary>
    /// The Unit.
    /// </summary>
    public static readonly Unit Default = new();

    private Unit() { }

    /// <inheritdoc />
    public string Name => "Unit";

    /// <inheritdoc />
    public string Serialize() => Name;

    /// <inheritdoc />
    public TypeReference TypeReference => TypeReference.Unit.Instance;
}
