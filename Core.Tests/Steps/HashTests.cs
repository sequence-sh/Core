namespace Reductech.Sequence.Core.Tests.Steps;

public partial class HashTests : StepTestBase<Hash, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Test md5",
                new Hash() { Data = Constant("abc") },
                "�P�<�O�֖?}(�r"
            );
        }
    }
}
