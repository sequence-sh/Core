namespace Sequence.Core.Tests.Steps;

public partial class LogTests : StepTestBase<Log, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Log something",
                new Log() { Value = Constant("Hello") },
                Unit.Default,
                "Hello"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Named argument",
                "Log Value: 'Hello'",
                Unit.Default,
                "Hello"
            );

            yield return new DeserializeCase(
                "Ordered Argument",
                "Log 'Hello'",
                Unit.Default,
                "Hello"
            );
        }
    }
}
