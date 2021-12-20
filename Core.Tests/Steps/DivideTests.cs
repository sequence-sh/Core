namespace Reductech.EDR.Core.Tests.Steps;

public partial class DivideTests : StepTestBase<Divide, SCLInt>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No Terms",
                new Divide()
                {
                    Terms = new ArrayNew<SCLInt>() { Elements = new List<IStep<SCLInt>>() }
                },
                0.ConvertToSCLObject()
            );

            yield return new StepCase(
                "One number",
                new Divide() { Terms = Array(2) },
                2.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two Terms",
                new Divide() { Terms = Array(6, 3) },
                2.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Three Terms",
                new Divide() { Terms = Array(24, 3, 4) },
                2.ConvertToSCLObject()
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            var case1 = CreateDefaultSerializeCase();

            yield return case1 with { ExpectedSCL = "0 / 1 / 2" };
        }
    }
}
