using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class DoubleSumTests : StepTestBase<DoubleSum, double>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No numbers",
                new DoubleSum()
                {
                    Terms = new ArrayNew<double>() { Elements = new List<IStep<double>>() }
                },
                0
            );

            yield return new StepCase(
                "One number",
                new DoubleSum() { Terms = StaticHelpers.Array(2d) },
                2d
            );

            yield return new StepCase(
                "Two numbers",
                new DoubleSum() { Terms = StaticHelpers.Array(2.5d, 3.5d) },
                6
            );

            yield return new StepCase(
                "Three numbers",
                new DoubleSum() { Terms = StaticHelpers.Array(2.2d, 3.3d, 4.4d) },
                9.9
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
                @"0 + 1.1 + 2.2"
            );
        }
    }
}

}
