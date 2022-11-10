namespace Sequence.Core.Tests.Steps;

public partial class StringIsEmptyTests : StepTestBase<StringIsEmpty, SCLBool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "String is empty",
                new StringIsEmpty() { String = Constant("") },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "String is not empty",
                new StringIsEmpty() { String = Constant("Hello") },
                false.ConvertToSCLObject()
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "String is empty",
                "StringIsEmpty String: ''",
                true.ConvertToSCLObject()
            );

            yield return new DeserializeCase(
                "String is not empty",
                "StringIsEmpty String: 'Hello'",
                false.ConvertToSCLObject()
            );
        }
    }
}
