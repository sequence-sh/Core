namespace Sequence.Core.Internal;

/// <summary>
/// An injected variable
/// </summary>
public sealed record InjectedVariable(
    ISCLObject SCLObject,
    string? Description);
