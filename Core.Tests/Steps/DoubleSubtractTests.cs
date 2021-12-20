namespace Reductech.Sequence.Core.Tests.Steps;

public partial class DoubleSubtractTests : StepTestBase<DoubleSubtract, SCLDouble>
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
                    Terms = new ArrayNew<SCLDouble>()
                    {
                        Elements = new List<IStep<SCLDouble>>()
                    }
                },
                0.0.ConvertToSCLObject()
            );

            yield return new StepCase(
                "One number",
                new DoubleSubtract() { Terms = Array(2.2) },
                2.2.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers",
                new DoubleSubtract() { Terms = Array(2, 3.5) },
                (-1.5d).ConvertToSCLObject()
            );

            yield return new StepCase(
                "Three numbers",
                new DoubleSubtract() { Terms = Array(2.5, 3.25, 4.75) },
                (-5.5).ConvertToSCLObject()
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
