using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class DoublePowerTests : StepTestBase<DoublePower, double>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No numbers",
                new DoublePower()
                {
                    Terms = new ArrayNew<double>() { Elements = new List<IStep<double>>() }
                },
                0
            );

            yield return new StepCase(
                "One number",
                new DoublePower() { Terms = StaticHelpers.Array(2.1) },
                2.1
            );

            yield return new StepCase(
                "Two numbers",
                new DoublePower() { Terms = StaticHelpers.Array(4, 2.5) },
                32
            );

            yield return new StepCase(
                "Three numbers",
                new DoublePower() { Terms = StaticHelpers.Array(2, 2, 2.5) },
                32
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
                @"0 ^ 1.1 ^ 2.2"
            );
        }
    }
}
