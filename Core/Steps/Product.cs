namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Calculate the product of a list of integers
/// </summary>
public sealed class Product : BaseOperatorStep<Product, SCLInt, SCLInt>
{
    /// <inheritdoc />
    protected override Result<SCLInt, IErrorBuilder> Operate(IEnumerable<SCLInt> terms)
    {
        return terms.Select(x => x.Value).Aggregate(1, (a, b) => a * b).ConvertToSCLObject();
    }

    /// <inheritdoc />
    public override string Operator => "*";
}
