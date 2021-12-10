using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class RepeatTests : StepTestBase<Repeat<int>, Array<int>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Repeat number",
                new Repeat<int>() { Element = Constant(6), Number = Constant(3) },
                new List<int>() { 6, 6, 6 }.ToSCLArray()
            );

            yield return new StepCase(
                "Repeat zero times",
                new Repeat<int>() { Element = Constant(6), Number = Constant(0) },
                new List<int>().ToSCLArray()
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Repeat number",
                "Repeat Element: 6 Number: 3",
                new List<int>() { 6, 6, 6 }.ToSCLArray()
            );
        }
    }
}
