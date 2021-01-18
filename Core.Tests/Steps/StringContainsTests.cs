using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class StringContainsTests : StepTestBase<StringContains, bool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "True case sensitive",
                new StringContains
                {
                    String = Constant("Hello World"), Substring = Constant("Hello")
                },
                true
            );

            yield return new StepCase(
                "False case sensitive",
                new StringContains
                {
                    String = Constant("Hello World"), Substring = Constant("hello")
                },
                false
            );

            yield return new StepCase(
                "True case insensitive",
                new StringContains
                {
                    String     = Constant("Hello World"),
                    Substring  = Constant("hello"),
                    IgnoreCase = Constant(true)
                },
                true
            );

            yield return new StepCase(
                "False case insensitive",
                new StringContains
                {
                    String     = Constant("Hello World"),
                    Substring  = Constant("Goodbye"),
                    IgnoreCase = Constant(true)
                },
                false
            );
        }
    }
}

}
