using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ModuloTests : StepTestBase<Modulo, int>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No numbers",
                new Modulo()
                {
                    Numbers = new ArrayNew<int>() { Elements = new List<IStep<int>>() }
                },
                0
            );

            yield return new StepCase(
                "One number",
                new Modulo() { Numbers = StaticHelpers.Array(2) },
                2
            );

            yield return new StepCase(
                "Two numbers",
                new Modulo() { Numbers = StaticHelpers.Array(5, 3) },
                2
            );

            yield return new StepCase(
                "Three numbers",
                new Modulo() { Numbers = StaticHelpers.Array(14, 5, 3) },
                1
            );
        }
    }
}

}
