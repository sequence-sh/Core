namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Calculate the sum of a list of doubles
/// </summary>
public sealed class DoubleSum : BaseOperatorStep<DoubleSum, double, double>
{
    /// <inheritdoc />
    public override string Operator => "+";

    /// <inheritdoc />
    protected override Result<double, IErrorBuilder> Operate(IEnumerable<double> terms)
    {
        return terms.Sum();
    }
}
