namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Calculate the sum of a list of doubles
/// </summary>
public sealed class DoubleSum : BaseOperatorStep<DoubleSum, SCLDouble, SCLDouble>
{
    /// <inheritdoc />
    public override string Operator => "+";

    /// <inheritdoc />
    protected override Result<SCLDouble, IErrorBuilder> Operate(IEnumerable<SCLDouble> terms)
    {
        return terms.Select(x => x.Value).Sum().ConvertToSCLObject();
    }
}
