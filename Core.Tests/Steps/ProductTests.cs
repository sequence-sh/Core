namespace Reductech.Sequence.Core.Tests.Steps;

public partial class ProductTests : StepTestBase<Product, SCLInt>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No numbers",
                new Product()
                {
                    Terms = new ArrayNew<SCLInt>() { Elements = new List<IStep<SCLInt>>() }
                },
                1.ConvertToSCLObject()
            );

            yield return new StepCase(
                "One number",
                new Product() { Terms = Array(2) },
                2.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers",
                new Product() { Terms = Array(2, 3) },
                6.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Three numbers",
                new Product() { Terms = Array(2, 3, 4) },
                24.ConvertToSCLObject()
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
                @"0 * 1 * 2"
            );
        }
    }
}
