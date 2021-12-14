namespace Reductech.EDR.Core.Tests.Steps;

public partial class DoNothingTests : StepTestBase<DoNothing, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase("Do nothing", new DoNothing(), Unit.Default);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase("Do nothing", "DoNothing", Unit.Default);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases => new List<ErrorCase>();
}
