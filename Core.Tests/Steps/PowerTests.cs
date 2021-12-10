using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class PowerTests : StepTestBase<Power, int>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No numbers",
                new Power() { Terms = new ArrayNew<int>() { Elements = new List<IStep<int>>() } },
                0
            );

            yield return new StepCase(
                "One number",
                new Power() { Terms = StaticHelpers.Array(2) },
                2
            );

            yield return new StepCase(
                "Two numbers",
                new Power() { Terms = StaticHelpers.Array(2, 3) },
                8
            );

            yield return new StepCase(
                "Three numbers",
                new Power() { Terms = StaticHelpers.Array(2, 3, 4) },
                4096
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            var (step, _) = CreateStepWithDefaultOrArbitraryValues();

            yield return new SerializeCase(
                "Default",
                step,
                @"0 ^ 1 ^ 2"
            );
        }
    }
}
