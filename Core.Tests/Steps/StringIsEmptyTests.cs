using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class StringIsEmptyTests : StepTestBase<StringIsEmpty, bool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "String is empty",
                new StringIsEmpty() { String = Constant("") },
                true
            );

            yield return new StepCase(
                "String is not empty",
                new StringIsEmpty() { String = Constant("Hello") },
                false
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase("String is empty", "StringIsEmpty String: ''", true);

            yield return new DeserializeCase(
                "String is not empty",
                "StringIsEmpty String: 'Hello'",
                false
            );
        }
    }
}
