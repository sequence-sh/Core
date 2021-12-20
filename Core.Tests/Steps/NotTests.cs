namespace Reductech.Sequence.Core.Tests.Steps;

public partial class NotTests : StepTestBase<Not, SCLBool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Not True",
                new Not { Boolean = Constant(true) },
                false.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Not False",
                new Not { Boolean = Constant(false) },
                true.ConvertToSCLObject()
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Ordered argument",
                "not true",
                false.ConvertToSCLObject()
            );

            yield return new DeserializeCase(
                "Named argument",
                "not boolean: true",
                false.ConvertToSCLObject()
            );
        }
    }
}
