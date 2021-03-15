using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Raises a double to the power of a list of integers sequentially
/// </summary>
public sealed class DoublePower : BaseOperatorStep<DoublePower, double, double>
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
                total = Math.Pow(total, number);
            }
        }

        return total;
    }

    /// <inheritdoc />
    public override string Operator => "^";
}

}
