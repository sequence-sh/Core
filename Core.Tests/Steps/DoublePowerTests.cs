namespace Sequence.Core.Tests.Steps;

public partial class DoublePowerTests : StepTestBase<DoublePower, SCLDouble>
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
                    Terms = new ArrayNew<SCLDouble>()
                    {
                        Elements = new List<IStep<SCLDouble>>()
                    }
                },
                0.0.ConvertToSCLObject()
            );

            yield return new StepCase(
                "One number",
                new DoublePower() { Terms = Array(2.1) },
                2.1.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers",
                new DoublePower() { Terms = Array(4, 2.5) },
                32.0.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Three numbers",
                new DoublePower() { Terms = Array(2, 2, 2.5) },
                32.0.ConvertToSCLObject()
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
