using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class RegexMatchTests : StepTestBase<RegexMatch, bool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Simple true match",
                new RegexMatch()
                {
                    String     = Constant("hello"),
                    Pattern    = Constant(@"h\w+"),
                    IgnoreCase = Constant(false)
                },
                true
            );

            yield return new StepCase(
                "Simple false match",
                new RegexMatch()
                {
                    String     = Constant("hello"),
                    Pattern    = Constant(@"H\w+"),
                    IgnoreCase = Constant(false)
                },
                false
            );

            yield return new StepCase(
                "Ignore case true match",
                new RegexMatch()
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

}
