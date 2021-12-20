namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Returns true if each term is greater than or equals to the next term
/// </summary>
public sealed class GreaterThanOrEqual<T> : CompareBaseOperatorStep<GreaterThanOrEqual<T>, T>
    where T : ISCLObject
{
    /// <inheritdoc />
    protected override bool CheckComparisonValue(int v) => v >= 0;

    /// <inheritdoc />
    public override string Operator => ">=";
}
