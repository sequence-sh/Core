using System;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Returns true if each term is less than the next term
/// </summary>
public sealed class LessThan<T> : CompareBaseOperatorStep<LessThan<T>, T>
    where T : IComparable<T>
{
    /// <inheritdoc />
    protected override bool CheckComparisonValue(int v) => v < 0;
}

}
