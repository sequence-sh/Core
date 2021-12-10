using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class DoubleSubtractTests : StepTestBase<DoubleSubtract, double>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No numbers",
                new DoubleSubtract()
                {
                    Terms = new ArrayNew<double>() { Elements = new List<IStep<double>>() }
                },
                0
            );

            yield return new StepCase(
                "One number",
                new DoubleSubtract() { Terms = StaticHelpers.Array(2.2) },
                2.2
            );

            yield return new StepCase(
                "Two numbers",
                new DoubleSubtract() { Terms = StaticHelpers.Array(2, 3.5) },
                -1.5
            );

            yield return new StepCase(
                "Three numbers",
                new DoubleSubtract() { Terms = StaticHelpers.Array(2.5, 3.25, 4.75) },
                -5.5
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
                @"0 - 1.1 - 2.2"
            );
        }
    }
}
