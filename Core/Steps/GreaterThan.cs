namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Returns true if each term is less than the next term
/// </summary>
public sealed class GreaterThan<T> : CompareBaseOperatorStep<GreaterThan<T>, T> where T : ISCLObject
{
    /// <inheritdoc />
    protected override bool CheckComparisonValue(int v) => v > 0;

    /// <inheritdoc />
    public override string Operator => ">";
}
