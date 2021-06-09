using System;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Returns true is all terms are equal
/// </summary>
public sealed class Equals<T> : CompareBaseOperatorStep<Equals<T>, T>
{
    /// <inheritdoc />
    protected override bool CheckComparisonValue(int v) => v == 0;

    /// <inheritdoc />
    public override string Operator => "==";
}

}
