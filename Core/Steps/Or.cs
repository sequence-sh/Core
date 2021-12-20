namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Returns true if any terms are true
/// </summary>
public sealed class Or : BaseOperatorStep<Or, SCLBool, SCLBool>
{
    /// <inheritdoc />
    protected override Result<SCLBool, IErrorBuilder> Operate(IEnumerable<SCLBool> terms)
    {
        return terms.Any(x => x.Value).ConvertToSCLObject();
    }

    /// <inheritdoc />
    public override string Operator => "||";
}
