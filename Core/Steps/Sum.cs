namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Calculate the sum of a list of integers
/// </summary>
[Alias("Add")]
public sealed class Sum : BaseOperatorStep<Sum, SCLInt, SCLInt>
{
    /// <inheritdoc />
    public override string Operator => "+";

    /// <inheritdoc />
    protected override Result<SCLInt, IErrorBuilder> Operate(IEnumerable<SCLInt> terms)
    {
        return terms.Sum(x => x.Value).ConvertToSCLObject();
    }
}
