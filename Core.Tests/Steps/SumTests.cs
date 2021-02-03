using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class SumTests : StepTestBase<Sum, int>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No numbers",
                new Sum() { Terms = new ArrayNew<int>() { Elements = new List<IStep<int>>() } },
                0
            );

            yield return new StepCase(
                "One number",
                new Sum() { Terms = Array(2) },
                2
            );

            yield return new StepCase(
                "Two numbers",
                new Sum() { Terms = Array(2, 3) },
                5
            );

            yield return new StepCase(
                "Three numbers",
                new Sum() { Terms = Array(2, 3, 4) },
                9
            );
        }
    }
}

}
