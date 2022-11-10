namespace Sequence.Core.Tests.Steps;

public partial class DoubleProductTests : StepTestBase<DoubleProduct, SCLDouble>
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
                    Terms = new ArrayNew<SCLDouble>()
                    {
                        Elements = new List<IStep<SCLDouble>>()
                    }
                },
                1d.ConvertToSCLObject()
            );

            yield return new StepCase(
                "One number",
                new DoubleProduct() { Terms = Array(2.2) },
                2.2.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers",
                new DoubleProduct() { Terms = Array(2.2, 3.3) },
                7.26.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Three numbers",
                new DoubleProduct() { Terms = Array(2.2, 3.3, 4) },
                29.04.ConvertToSCLObject()
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
