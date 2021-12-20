namespace Reductech.Sequence.Core.Tests.Steps;

public partial class GreaterThanTests : StepTestBase<GreaterThan<SCLInt>, SCLBool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "One number",
                new GreaterThan<SCLInt>() { Terms = Array(2) },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers false",
                new GreaterThan<SCLInt>() { Terms = Array(2, 3) },
                false.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers true",
                new GreaterThan<SCLInt>() { Terms = Array(3, 2) },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Three numbers true",
                new GreaterThan<SCLInt>() { Terms = Array(3, 2, 1) },
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
                @"0 > 1 > 2"
            );
        }
    }
}
