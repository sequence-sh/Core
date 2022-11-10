namespace Sequence.Core.Tests.Steps;

public partial class SplitStringTests : StepTestBase<StringSplit, Array<StringStream>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Split a string",
                new StringSplit() { String = Constant("Hello World"), Delimiter = Constant(" ") },
                new List<StringStream>() { "Hello", "World" }.ToSCLArray()
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Split a string",
                "StringSplit String: 'Hello World' Delimiter: ' '",
                new List<StringStream> { "Hello", "World" }.ToSCLArray()
            );
        }
    }
}
