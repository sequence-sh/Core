using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Returns true if all terms are true
/// </summary>
[SCLExample("true && true",         "True")]
[SCLExample("true && false",        "False")]
[SCLExample("true && true && true", "True")]
[SCLExample("(3 > 2) && (2 > 1)",   "True")]
public sealed class And : BaseOperatorStep<And, bool, bool>
{
    /// <inheritdoc />
    protected override Result<bool, IErrorBuilder> Operate(IEnumerable<bool> terms)
    {
        return terms.All(x => x);
    }

    /// <inheritdoc />
    public override string Operator => "&&";
}
