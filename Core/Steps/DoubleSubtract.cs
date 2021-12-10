namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Subtract a list of numbers from a number
/// </summary>
public sealed class DoubleSubtract : BaseOperatorStep<DoubleSubtract, SCLDouble, SCLDouble>
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
                total -= number.Value;
            }
        }

        return total.ConvertToSCLObject();
    }

    /// <inheritdoc />
    public override string Operator => "-";
}
