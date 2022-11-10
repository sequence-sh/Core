namespace Sequence.Core.Tests.Steps;

public partial class GreaterThanOrEqualTests : StepTestBase<GreaterThanOrEqual<SCLInt>, SCLBool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "One number",
                new GreaterThanOrEqual<SCLInt>() { Terms = Array(2) },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers false",
                new GreaterThanOrEqual<SCLInt>() { Terms = Array(1, 2) },
                false.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers true",
                new GreaterThanOrEqual<SCLInt>() { Terms = Array(3, 2) },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Three numbers true",
                new GreaterThanOrEqual<SCLInt>() { Terms = Array(3, 2, 2) },
                true.ConvertToSCLObject()
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
                @"0 >= 1 >= 2"
            );
        }
    }
}
