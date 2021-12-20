namespace Reductech.Sequence.Core.Tests.Steps;

public partial class StringLengthTests : StepTestBase<StringLength, SCLInt>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "simple length of string",
                new StringLength() { String = Constant("Hello") },
                5.ConvertToSCLObject()
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Simple length of string",
                "StringLength String: 'Hello'",
                5.ConvertToSCLObject()
            );
        }
    }
}
