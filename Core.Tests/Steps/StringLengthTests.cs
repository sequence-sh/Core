using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class StringLengthTests : StepTestBase<StringLength, int>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "simple length of string",
                new StringLength() { String = Constant("Hello") },
                5
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
                5
            );
        }
    }
}
