namespace Reductech.Sequence.Core.Tests.Steps;

public partial class ArraySkipTests : StepTestBase<ArraySkip<SCLInt>, Array<SCLInt>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Take Elements",
                new ArraySkip<SCLInt>() { Array = Array(1, 2, 3), Count = Constant(2) },
                new[] { 3 }.Select(x => x.ConvertToSCLObject()).ToSCLArray()
            );
        }
    }
}
