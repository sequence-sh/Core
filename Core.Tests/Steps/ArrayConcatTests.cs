using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ArrayConcatTests2 : StepTestBase<ArrayConcat<int>, Array<int>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get { yield break; }
    }
}

}
