using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Calculate the product of a list of doubles
/// </summary>
public sealed class DoubleProduct : BaseOperatorStep<DoubleProduct, double, double>
{
    /// <inheritdoc />
    protected override Result<double, IErrorBuilder> Operate(IEnumerable<double> terms)
    {
        return terms.Aggregate(1d, (a, b) => a * b);
    }

    /// <inheritdoc />
    public override string Operator => "*";
}

}
