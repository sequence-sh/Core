namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Calculate the product of a list of doubles
/// </summary>
public sealed class DoubleProduct : BaseOperatorStep<DoubleProduct, SCLDouble, SCLDouble>
{
    /// <inheritdoc />
    protected override Result<SCLDouble, IErrorBuilder> Operate(IEnumerable<SCLDouble> terms)
    {
        return terms.Select(x => x.Value).Aggregate(1d, (a, b) => a * b).ConvertToSCLObject();
    }

    /// <inheritdoc />
    public override string Operator => "*";
}
