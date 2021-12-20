namespace Reductech.Sequence.Core.Tests.Steps;

public partial class DoubleSumTests : StepTestBase<DoubleSum, SCLDouble>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No numbers",
                new DoubleSum()
                {
                    Terms = new ArrayNew<SCLDouble>()
                    {
                        Elements = new List<IStep<SCLDouble>>()
                    }
                },
                0d.ConvertToSCLObject()
            );

            yield return new StepCase(
                "One number",
                new DoubleSum() { Terms = Array(2d) },
                2d.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers",
                new DoubleSum() { Terms = Array(2.5d, 3.5d) },
                6d.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Three numbers",
                new DoubleSum() { Terms = Array(2.2d, 3.3d, 4.4d) },
                9.9d.ConvertToSCLObject()
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
                @"0 + 1.1 + 2.2"
            );
        }
    }
}
