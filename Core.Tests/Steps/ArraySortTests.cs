using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ArraySortTests : StepTestBase<ArraySort<int>, Array<int>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Ascending",
                new ArraySort<int>()
                {
                    Array = Array(8, 6, 7, 5, 3, 0, 9), Descending = Constant(false)
                },
                ArrayHelper.ToArray(
                    new List<int>()
                    {
                        0,
                        3,
                        5,
                        6,
                        7,
                        8,
                        9
                    }
                )
            );

            yield return new StepCase(
                "Descending",
                new ArraySort<int>()
                {
                    Array = Array(8, 6, 7, 5, 3, 0, 9), Descending = Constant(true)
                },
                ArrayHelper.ToArray(
                    new List<int>()
                    {
                        9,
                        8,
                        7,
                        6,
                        5,
                        3,
                        0
                    }
                )
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Sort Ascending",
                "ArraySort Array: [8,6,7,5,3,0,9]",
                ArrayHelper.ToArray(
                    new List<int>()
                    {
                        0,
                        3,
                        5,
                        6,
                        7,
                        8,
                        9
                    }
                )
            );
        }
    }
}

}
