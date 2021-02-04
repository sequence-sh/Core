using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Calculate the sum of a list of numbers
/// </summary>
[Alias("Add")]
public sealed class Sum : BaseOperatorStep<Sum, int, int>
{
    /// <inheritdoc />
    public override string Operator => "+";

    /// <inheritdoc />
    protected override Result<int, IErrorBuilder> Operate(IEnumerable<int> terms)
    {
        return terms.Sum();
    }
}

}
