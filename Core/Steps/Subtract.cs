namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Subtract a list of numbers from a number
/// </summary>
public sealed class Subtract : BaseOperatorStep<Subtract, SCLInt, SCLInt>
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
                total -= number.Value;
            }
        }

        return total.ConvertToSCLObject();
    }

    /// <inheritdoc />
    public override string Operator => "-";
}
