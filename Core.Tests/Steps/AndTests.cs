using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class AndTests : StepTestBase<And, bool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No terms",
                new And() { Terms = new ArrayNew<bool>() { Elements = new List<IStep<bool>>() } },
                true
            );

            yield return new StepCase(
                "just true",
                new And() { Terms = StaticHelpers.Array(true) },
                true
            );

            yield return new StepCase(
                "just false",
                new And() { Terms = StaticHelpers.Array(false) },
                false
            );

            yield return new StepCase(
                "three trues",
                new And() { Terms = StaticHelpers.Array(true, true, true) },
                true
            );
        }
    }
}

}
