namespace Reductech.EDR.Core.Tests.Steps;

public partial class LessThanTests : StepTestBase<LessThan<SCLInt>, SCLBool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "One number",
                new LessThan<SCLInt>() { Terms = Array(2) },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers false",
                new LessThan<SCLInt>() { Terms = Array(2, 2) },
                false.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers true",
                new LessThan<SCLInt>() { Terms = Array(1, 2) },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Three numbers true",
                new LessThan<SCLInt>() { Terms = Array(1, 2, 3) },
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
                @"0 < 1 < 2"
            );
        }
    }
}
