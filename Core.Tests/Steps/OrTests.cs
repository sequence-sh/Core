using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

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
                new Or() { Terms = StaticHelpers.Array(true) },
                true
            );

            yield return new StepCase(
                "just false",
                new Or() { Terms = StaticHelpers.Array(false) },
                false
            );

            yield return new StepCase(
                "three falses",
                new Or() { Terms = StaticHelpers.Array(false, false, false) },
                false
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
                @"True || False || True"
            );
        }
    }
}
