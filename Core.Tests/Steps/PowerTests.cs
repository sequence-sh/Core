namespace Reductech.Sequence.Core.Tests.Steps;

public partial class PowerTests : StepTestBase<Power, SCLInt>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No numbers",
                new Power()
                {
                    Terms = new ArrayNew<SCLInt>() { Elements = new List<IStep<SCLInt>>() }
                },
                0.ConvertToSCLObject()
            );

            yield return new StepCase(
                "One number",
                new Power() { Terms = Array(2) },
                2.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers",
                new Power() { Terms = Array(2, 3) },
                8.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Three numbers",
                new Power() { Terms = Array(2, 3, 4) },
                4096.ConvertToSCLObject()
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
