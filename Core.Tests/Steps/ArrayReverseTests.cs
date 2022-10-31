namespace Reductech.Sequence.Core.Tests.Steps;

public partial class ArrayReverseTests : StepTestBase<ArrayReverse<SCLInt>, Array<SCLInt>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases { get; } = new List<StepCase>();
}
