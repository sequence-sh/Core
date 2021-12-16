namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Divide an integer by a list of integers
/// </summary>
[SCLExample("5 / 2", "2")]
public sealed class Divide : BaseOperatorStep<Divide, SCLInt, SCLInt>
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
