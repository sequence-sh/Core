namespace Reductech.Sequence.Core.Tests.Steps;

public partial class EqualsTests : StepTestBase<Equals<SCLInt>, SCLBool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "One number",
                new Equals<SCLInt>() { Terms = Array(2) },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers false",
                new Equals<SCLInt>() { Terms = Array(2, 3) },
                false.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers true",
                new Equals<SCLInt>() { Terms = Array(2, 2) },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Three numbers true",
                new Equals<SCLInt>() { Terms = Array(2, 2, 2) },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Steps are operators",
                new Equals<SCLInt>()
                {
                    Terms = new ArrayNew<SCLInt>()
                    {
                        Elements = new List<IStep<SCLInt>>()
                        {
                            new Sum() { Terms     = Array(2, 2) },
                            new Product() { Terms = Array(2, 2) },
                        }
                    }
                },
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
                @"0 == 1 == 2"
            );
        }
    }
}
