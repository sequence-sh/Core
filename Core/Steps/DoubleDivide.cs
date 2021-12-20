namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Divide a double by a list of doubles
/// </summary>
public sealed class DoubleDivide : BaseOperatorStep<DoubleDivide, SCLDouble, SCLDouble>
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
                if (number.Value == 0)
                    return ErrorCode.DivideByZero.ToErrorBuilder();

                total /= number.Value;
            }
        }

        return total.ConvertToSCLObject();
    }

    /// <inheritdoc />
    public override string Operator => "/";
}
