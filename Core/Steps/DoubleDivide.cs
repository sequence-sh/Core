using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Divide a double by a list of doubles
/// </summary>
public sealed class DoubleDivide : BaseOperatorStep<DoubleDivide, double, double>
{
    /// <inheritdoc />
    protected override Result<double, IErrorBuilder> Operate(IEnumerable<double> terms)
    {
        double total = 0;
        var    first = true;

        foreach (var number in terms)
        {
            if (first)
            {
                total += number;
                first =  false;
            }
            else
            {
                if (number == 0)
                    return ErrorCode.DivideByZero.ToErrorBuilder();

                total /= number;
            }
        }

        return total;
    }

    /// <inheritdoc />
    public override string Operator => "/";
}

}
