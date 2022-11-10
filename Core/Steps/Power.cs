namespace Sequence.Core.Steps;

/// <summary>
/// Raises an integer to the power of a list of integers sequentially
/// </summary>
[AllowConstantFolding]
public sealed class Power : BaseOperatorStep<Power, SCLInt, SCLInt>
{
    /// <inheritdoc />
    protected override Result<SCLInt, IErrorBuilder> Operate(IEnumerable<SCLInt> terms)
    {
        var total = 0;
        var first = true;

        foreach (var number in terms)
        {
            if (first)
            {
                total += number.Value;
                first =  false;
            }
            else
            {
                total = Convert.ToInt32(Math.Pow(total, number.Value));
            }
        }

        return total.ConvertToSCLObject();
    }

    /// <inheritdoc />
    public override string Operator => "^";
}
