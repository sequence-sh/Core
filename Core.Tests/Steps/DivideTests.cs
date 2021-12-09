using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class DivideTests : StepTestBase<Divide, int>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No Terms",
                new Divide() { Terms = new ArrayNew<int>() { Elements = new List<IStep<int>>() } },
                0
            );

            yield return new StepCase(
                "One number",
                new Divide() { Terms = StaticHelpers.Array(2) },
                2
            );

            yield return new StepCase(
                "Two Terms",
                new Divide() { Terms = StaticHelpers.Array(6, 3) },
                2
            );

            yield return new StepCase(
                "Three Terms",
                new Divide() { Terms = StaticHelpers.Array(24, 3, 4) },
                2
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            var case1 = CreateDefaultSerializeCase();

            yield return case1 with { ExpectedSCL = "0 / 1 / 2" };
        }
    }
}
