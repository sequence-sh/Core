namespace Reductech.Sequence.Core.Tests.Steps;

public partial class AndTests : StepTestBase<And, SCLBool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No terms",
                new And()
                {
                    Terms = new ArrayNew<SCLBool>() { Elements = new List<IStep<SCLBool>>() }
                },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "just true",
                new And() { Terms = Array(true) },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "just false",
                new And() { Terms = Array(false) },
                false.ConvertToSCLObject()
            );

            yield return new StepCase(
                "three trues",
                new And() { Terms = Array(true, true, true) },
                true.ConvertToSCLObject()
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            var case1 = CreateDefaultSerializeCase();

            yield return case1 with { ExpectedSCL = "True && False && True" };
        }
    }
}
