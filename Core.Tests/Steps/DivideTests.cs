using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class DivideTests : StepTestBase<Divide, int>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No numbers",
                new Divide()
                {
                    Numbers = new ArrayNew<int>() { Elements = new List<IStep<int>>() }
                },
                0
            );

            yield return new StepCase(
                "One number",
                new Divide() { Numbers = StaticHelpers.Array(2) },
                2
            );

            yield return new StepCase(
                "Two numbers",
                new Divide() { Numbers = StaticHelpers.Array(6, 3) },
                2
            );

            yield return new StepCase(
                "Three numbers",
                new Divide() { Numbers = StaticHelpers.Array(24, 3, 4) },
                2
            );
        }
    }
}

}
