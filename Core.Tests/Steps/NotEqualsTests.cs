using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class NotEqualsTests : StepTestBase<NotEquals<int>, bool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "One number",
                new NotEquals<int>() { Terms = StaticHelpers.Array(2) },
                true
            );

            yield return new StepCase(
                "Two numbers false",
                new NotEquals<int>() { Terms = StaticHelpers.Array(2, 2) },
                false
            );

            yield return new StepCase(
                "Two numbers true",
                new NotEquals<int>() { Terms = StaticHelpers.Array(2, 3) },
                true
            );

            yield return new StepCase(
                "Three numbers true",
                new NotEquals<int>() { Terms = StaticHelpers.Array(5, 3, 4) },
                true
            );
        }
    }
}

}
