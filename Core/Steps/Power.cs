using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Raises a number to the power of a list of numbers sequentially
/// </summary>
public sealed class Power : BaseOperatorStep<Power, int, int>
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
                total = Convert.ToInt32(Math.Pow(total, number));
            }
        }

        return total;
    }
}

}
