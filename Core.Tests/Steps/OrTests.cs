namespace Sequence.Core.Tests.Steps;

public partial class OrTests : StepTestBase<Or, SCLBool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No terms",
                new Or()
                {
                    Terms = new ArrayNew<SCLBool>() { Elements = new List<IStep<SCLBool>>() }
                },
                false.ConvertToSCLObject()
            );

            yield return new StepCase(
                "just true",
                new Or() { Terms = Array(true) },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "just false",
                new Or() { Terms = Array(false) },
                false.ConvertToSCLObject()
            );

            yield return new StepCase(
                "three falses",
                new Or() { Terms = Array(false, false, false) },
                false.ConvertToSCLObject()
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
                @"True || False || True"
            );
        }
    }
}
