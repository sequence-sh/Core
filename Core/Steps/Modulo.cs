namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Modulo a number by a list of integers sequentially
/// </summary>
public sealed class Modulo : BaseOperatorStep<Modulo, SCLInt, SCLInt>
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
                total %= number.Value;
            }
        }

        return total.ConvertToSCLObject();
    }

    /// <inheritdoc />
    public override string Operator => "%";
}
