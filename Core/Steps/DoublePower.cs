namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Raises a double to the power of a list of integers sequentially
/// </summary>
public sealed class DoublePower : BaseOperatorStep<DoublePower, SCLDouble, SCLDouble>
{
    /// <inheritdoc />
    protected override Result<SCLDouble, IErrorBuilder> Operate(IEnumerable<SCLDouble> terms)
    {
        double total = 0;
        var    first = true;

        foreach (var number in terms)
        {
            if (first)
            {
                total += number.Value;
                first =  false;
            }
            else
            {
                total = Math.Pow(total, number.Value);
            }
        }

        return total.ConvertToSCLObject();
    }

    /// <inheritdoc />
    public override string Operator => "^";
}
