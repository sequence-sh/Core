namespace Reductech.Sequence.Core.Tests.Steps;

public partial class SumTests : StepTestBase<Sum, SCLInt>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No numbers",
                new Sum()
                {
                    Terms = new ArrayNew<SCLInt>() { Elements = new List<IStep<SCLInt>>() }
                },
                0.ConvertToSCLObject()
            );

            yield return new StepCase(
                "One number",
                new Sum() { Terms = Array(2) },
                2.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers",
                new Sum() { Terms = Array(2, 3) },
                5.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Three numbers",
                new Sum() { Terms = Array(2, 3, 4) },
                9.ConvertToSCLObject()
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
                @"0 + 1 + 2"
            );
        }
    }
}
