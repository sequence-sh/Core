namespace Sequence.Core.Tests.Steps;

public partial class DoubleDivideTests : StepTestBase<DoubleDivide, SCLDouble>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No Terms",
                new DoubleDivide()
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
                new DoubleDivide() { Terms = Array(2.2) },
                2.2.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two Terms",
                new DoubleDivide() { Terms = Array(6.4, 3.2) },
                2.0.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Three Terms",
                new DoubleDivide() { Terms = Array(25.2, 3, 4) },
                2.1.ConvertToSCLObject()
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            var case1 = CreateDefaultSerializeCase();

            yield return case1 with { ExpectedSCL = "0 / 1.1 / 2.2" };
        }
    }
}
