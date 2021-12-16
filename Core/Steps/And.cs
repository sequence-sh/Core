namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Returns true if all terms are true
/// </summary>
[SCLExample("true && true",         "True")]
[SCLExample("true && false",        "False")]
[SCLExample("true && true && true", "True")]
[SCLExample("(3 > 2) && (2 > 1)",   "True")]
public sealed class And : BaseOperatorStep<And, SCLBool, SCLBool>
{
    /// <inheritdoc />
    protected override Result<SCLBool, IErrorBuilder> Operate(IEnumerable<SCLBool> terms)
    {
        return terms.All(x => x.Value).ConvertToSCLObject();
    }

    /// <inheritdoc />
    public override string Operator => "&&";
}
