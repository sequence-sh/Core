using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class DelayTests : StepTestBase<Delay, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Short Delay",
                new Delay() { Milliseconds = new IntConstant(10) },
                Unit.Default
            );
        }
    }
}
