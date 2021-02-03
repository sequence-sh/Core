using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Divide a number by a list of numbers
/// </summary>
public sealed class Divide : BaseOperatorStep<Divide, int, int>
{
    /// <inheritdoc />
    protected override Result<int, IErrorBuilder> Operate(IEnumerable<int> terms)
    {
        var total = 0;
        var first = true;

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
}

}
