using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

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
                new And() { Terms = Array(true) },
                true
            );

            yield return new StepCase(
                "just false",
                new And() { Terms = Array(false) },
                false
            );

            yield return new StepCase(
                "three trues",
                new And() { Terms = Array(true, true, true) },
                true
            );
        }
    }
}

public partial class OrTests : StepTestBase<Or, bool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No terms",
                new Or() { Terms = new ArrayNew<bool>() { Elements = new List<IStep<bool>>() } },
                false
            );

            yield return new StepCase(
                "just true",
                new Or() { Terms = Array(true) },
                true
            );

            yield return new StepCase(
                "just false",
                new Or() { Terms = Array(false) },
                false
            );

            yield return new StepCase(
                "three falses",
                new Or() { Terms = Array(false, false, false) },
                false
            );
        }
    }
}

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
