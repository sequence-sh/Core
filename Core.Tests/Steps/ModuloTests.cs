namespace Reductech.Sequence.Core.Tests.Steps;

public partial class ModuloTests : StepTestBase<Modulo, SCLInt>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No numbers",
                new Modulo()
                {
                    Terms = new ArrayNew<SCLInt>() { Elements = new List<IStep<SCLInt>>() }
                },
                0.ConvertToSCLObject()
            );

            yield return new StepCase(
                "One number",
                new Modulo() { Terms = Array(2) },
                2.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Two numbers",
                new Modulo() { Terms = Array(5, 3) },
                2.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Three numbers",
                new Modulo() { Terms = Array(14, 5, 3) },
                1.ConvertToSCLObject()
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
                @"0 % 1 % 2"
            );
        }
    }
}
