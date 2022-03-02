namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A variable being used somewhere in a sequence
/// </summary>
public sealed record UsedVariable(
    VariableName VariableName,
    TypeReference TypeReference,
    bool WasSet,
    TextLocation Location);
