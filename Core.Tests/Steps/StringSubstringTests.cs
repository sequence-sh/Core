namespace Reductech.Sequence.Core.Tests.Steps;

public partial class StringSubstringTests : StepTestBase<StringSubstring, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Substring length 3",
                new StringSubstring
                {
                    String = Constant("Hello World"), Index = Constant(1), Length = Constant(3)
                },
                "ell"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "ArrayLength 3",
                "StringSubstring String: 'Hello World' Index: 1 Length: 3",
                "ell"
            );
        }
    }
}
