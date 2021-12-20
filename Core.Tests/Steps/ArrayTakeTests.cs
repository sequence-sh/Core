namespace Reductech.Sequence.Core.Tests.Steps;

public partial class ArrayTakeTests : StepTestBase<ArrayTake<SCLInt>, Array<SCLInt>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Take Elements",
                new ArrayTake<SCLInt>() { Array = Array(1, 2, 3), Count = Constant(2) },
                new List<int>() { 1, 2 }.Select(x => x.ConvertToSCLObject()).ToSCLArray()
            );
        }
    }
}
