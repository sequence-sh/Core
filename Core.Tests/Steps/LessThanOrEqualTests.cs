namespace Reductech.Sequence.Core.Tests.Steps;

public partial class LessThanOrEqualTests : StepTestBase<LessThanOrEqual<SCLInt>, SCLBool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "One number",
                new LessThanOrEqual<SCLInt>() { Terms = Array(2) },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers false",
                new LessThanOrEqual<SCLInt>() { Terms = Array(4, 3) },
                false.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers true",
                new LessThanOrEqual<SCLInt>() { Terms = Array(1, 2) },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Three numbers true",
                new LessThanOrEqual<SCLInt>() { Terms = Array(2, 2, 3) },
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
                @"0 <= 1 <= 2"
            );
        }
    }
}
