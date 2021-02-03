using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Calculate the product of a list of numbers
/// </summary>
public sealed class Product : BaseOperatorStep<Product, int, int>
{
    /// <inheritdoc />
    protected override Result<int, IErrorBuilder> Operate(IEnumerable<int> terms)
    {
        return terms.Aggregate(1, (a, b) => a * b);
    }
}

}
