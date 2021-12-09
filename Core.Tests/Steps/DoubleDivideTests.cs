using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class DoubleDivideTests : StepTestBase<DoubleDivide, double>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No Terms",
                new DoubleDivide()
                {
                    Terms = new ArrayNew<double>() { Elements = new List<IStep<double>>() }
                },
                0
            );

            yield return new StepCase(
                "One number",
                new DoubleDivide() { Terms = StaticHelpers.Array(2.2) },
                2.2
            );

            yield return new StepCase(
                "Two Terms",
                new DoubleDivide() { Terms = StaticHelpers.Array(6.4, 3.2) },
                2
            );

            yield return new StepCase(
                "Three Terms",
                new DoubleDivide() { Terms = StaticHelpers.Array(25.2, 3, 4) },
                2.1
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            var case1 = CreateDefaultSerializeCase();

            yield return case1 with { ExpectedSCL = "0 / 1.1 / 2.2" };
        }
    }
}
