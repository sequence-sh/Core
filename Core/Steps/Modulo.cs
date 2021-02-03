using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Modulo a number by a list of numbers sequentially
/// </summary>
public sealed class Modulo : BaseOperatorStep<Modulo, int, int>
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
                total %= number;
            }
        }

        return total;
    }
}

}
