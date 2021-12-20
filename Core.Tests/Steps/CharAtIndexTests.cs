namespace Reductech.Sequence.Core.Tests.Steps;

public partial class CharAtIndexTests : StepTestBase<CharAtIndex, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Index is present",
                new CharAtIndex() { Index = Constant(1), String = Constant("Hello") },
                "e"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Index is present",
                "CharAtIndex Index: 1 String: 'Hello'",
                "e"
            );
        }
    }
}
