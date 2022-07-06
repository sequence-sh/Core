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
                "900150983CD24FB0D6963F7D28E17F72"
            );
        }
    }
}
