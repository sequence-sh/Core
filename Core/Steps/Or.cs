using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Returns true if any terms are true
/// </summary>
public sealed class Or : BaseOperatorStep<Or, bool, bool>
{
    /// <inheritdoc />
    protected override Result<bool, IErrorBuilder> Operate(IEnumerable<bool> terms)
    {
        return terms.Any(x => x);
    }

    /// <inheritdoc />
    public override string Operator => "||";
}

}
