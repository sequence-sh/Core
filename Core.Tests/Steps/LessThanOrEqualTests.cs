using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class LessThanOrEqualTests : StepTestBase<LessThanOrEqual<int>, bool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "One number",
                new LessThanOrEqual<int>() { Terms = StaticHelpers.Array(2) },
                true
            );

            yield return new StepCase(
                "Two numbers false",
                new LessThanOrEqual<int>() { Terms = StaticHelpers.Array(4, 3) },
                false
            );

            yield return new StepCase(
                "Two numbers true",
                new LessThanOrEqual<int>() { Terms = StaticHelpers.Array(1, 2) },
                true
            );

            yield return new StepCase(
                "Three numbers true",
                new LessThanOrEqual<int>() { Terms = StaticHelpers.Array(2, 2, 3) },
                true
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            var (step, _) = CreateStepWithDefaultOrArbitraryValues();

            yield return new SerializeCase(
                "Default",
                step,
                @"0 <= 1 <= 2"
            );
        }
    }
}

}
