namespace Reductech.Sequence.Core.Tests.Steps;

public partial class StringFindTests : StepTestBase<StringFind, SCLInt>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Substring is present",
                new StringFind() { String = Constant("Hello"), SubString = Constant("lo") },
                3.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Substring is no present",
                new StringFind() { String = Constant("Hello"), SubString = Constant("ol") },
                (-1).ConvertToSCLObject()
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Present",
                "StringFind String: 'Hello' Substring: 'lo'",
                3.ConvertToSCLObject()
            );
        }
    }
}
