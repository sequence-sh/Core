namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Calculate the product of a list of integers
/// </summary>
public sealed class Product : BaseOperatorStep<Product, int, int>
{
    /// <inheritdoc />
    protected override Result<int, IErrorBuilder> Operate(IEnumerable<int> terms)
    {
        return terms.Aggregate(1, (a, b) => a * b);
    }

    /// <inheritdoc />
    public override string Operator => "*";
}
