using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Returns true if all terms are true
/// </summary>
public sealed class And : BaseOperatorStep<And, bool, bool>
{
    /// <inheritdoc />
    protected override Result<bool, IErrorBuilder> Operate(IEnumerable<bool> terms)
    {
        return terms.All(x => x);
    }
}

}
