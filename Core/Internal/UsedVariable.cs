namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// A variable being used somewhere in a sequence
/// </summary>
public record UsedVariable(
    VariableName VariableName,
    TypeReference TypeReference,
    TextLocation Location);

}
