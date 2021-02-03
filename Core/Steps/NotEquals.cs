using System;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Returns true if each term is not equal to the next term
/// </summary>
public sealed class NotEquals<T> : CompareBaseOperatorStep<NotEquals<T>, T>
    where T : IComparable<T>
{
    /// <inheritdoc />
    protected override bool CheckComparisonValue(int v) => v != 0;
}

}
