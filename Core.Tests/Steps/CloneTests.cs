namespace Sequence.Core.Tests.Steps;

public partial class CloneTests : StepTestBase<Clone<SCLInt>, Array<SCLInt>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Clone number",
                new Clone<SCLInt>() { Element = Constant(6), Number = Constant(3) },
                new List<int>() { 6, 6, 6 }.Select(x => x.ConvertToSCLObject()).ToSCLArray()
            );

            yield return new StepCase(
                "Clone zero times",
                new Clone<SCLInt>() { Element = Constant(6), Number = Constant(0) },
                new List<int>().Select(x => x.ConvertToSCLObject()).ToSCLArray()
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Clone number",
                "Clone Element: 6 Number: 3",
                new List<int>() { 6, 6, 6 }.Select(x => x.ConvertToSCLObject()).ToSCLArray()
            );
        }
    }
}
