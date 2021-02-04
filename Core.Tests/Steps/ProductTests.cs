using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ProductTests : StepTestBase<Product, int>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No numbers",
                new Product() { Terms = new ArrayNew<int>() { Elements = new List<IStep<int>>() } },
                1
            );

            yield return new StepCase(
                "One number",
                new Product() { Terms = StaticHelpers.Array(2) },
                2
            );

            yield return new StepCase(
                "Two numbers",
                new Product() { Terms = StaticHelpers.Array(2, 3) },
                6
            );

            yield return new StepCase(
                "Three numbers",
                new Product() { Terms = StaticHelpers.Array(2, 3, 4) },
                24
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
                @"0 * 1 * 2"
            );
        }
    }
}

}
