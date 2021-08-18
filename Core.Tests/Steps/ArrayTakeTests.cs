using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ArrayTakeTests : StepTestBase<ArrayTake<int>, Array<int>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Take Elements",
                new ArrayTake<int>() { Array = Array(1, 2, 3), Count = Constant(2) },
                new EagerArray<int>(new List<int>() { 1, 2 })
            );
        }
    }
}

public partial class ArraySkipTests : StepTestBase<ArraySkip<int>, Array<int>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Take Elements",
                new ArraySkip<int>() { Array = Array(1, 2, 3), Count = Constant(2) },
                new EagerArray<int>(new List<int>() { 3 })
            );
        }
    }
}

}
