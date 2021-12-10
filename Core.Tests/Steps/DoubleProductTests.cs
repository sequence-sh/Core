using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class DoubleProductTests : StepTestBase<DoubleProduct, double>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No numbers",
                new DoubleProduct()
                {
                    Terms = new ArrayNew<double>() { Elements = new List<IStep<double>>() }
                },
                1
            );

            yield return new StepCase(
                "One number",
                new DoubleProduct() { Terms = StaticHelpers.Array(2.2) },
                2.2
            );

            yield return new StepCase(
                "Two numbers",
                new DoubleProduct() { Terms = StaticHelpers.Array(2.2, 3.3) },
                7.26
            );

            yield return new StepCase(
                "Three numbers",
                new DoubleProduct() { Terms = StaticHelpers.Array(2.2, 3.3, 4) },
                29.04
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
                @"0 * 1.1 * 2.2"
            );
        }
    }
}
