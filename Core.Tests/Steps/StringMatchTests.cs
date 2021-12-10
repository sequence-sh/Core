using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class StringMatchTests : StepTestBase<StringMatch, bool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Simple true match",
                new StringMatch()
                {
                    String     = Constant("hello"),
                    Pattern    = Constant(@"h\w+"),
                    IgnoreCase = Constant(false)
                },
                true
            );

            yield return new StepCase(
                "Simple false match",
                new StringMatch()
                {
                    String     = Constant("hello"),
                    Pattern    = Constant(@"H\w+"),
                    IgnoreCase = Constant(false)
                },
                false
            );

            yield return new StepCase(
                "Ignore case true match",
                new StringMatch()
                {
                    String     = Constant("hello"),
                    Pattern    = Constant(@"H\w+"),
                    IgnoreCase = Constant(true)
                },
                true
            );
        }
    }
}
