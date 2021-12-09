using System.Collections.Generic;
using System.Linq;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class ArraySortTests : StepTestBase<ArraySort<int>, Array<int>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            var elements = new[] { 8, 6, 7, 5, 3, 0, 9 };

            yield return new StepCase(
                "SortOrder False",
                new ArraySort<int>()
                {
                    Array      = Array(elements),
                    Descending = new OneOfStep<bool, SortOrder>(Constant(false))
                },
                elements.OrderBy(x => x).ToSCLArray()
            );

            yield return new StepCase(
                "SortOrder true",
                new ArraySort<int>()
                {
                    Array      = Array(elements),
                    Descending = new OneOfStep<bool, SortOrder>(Constant(true))
                },
                elements.OrderByDescending(x => x).ToSCLArray()
            );

            yield return new StepCase(
                "SortOrder Ascending",
                new ArraySort<int>()
                {
                    Array      = Array(elements),
                    Descending = new OneOfStep<bool, SortOrder>(Constant(SortOrder.Ascending))
                },
                elements.OrderBy(x => x).ToSCLArray()
            );

            yield return new StepCase(
                "SortOrder Descending",
                new ArraySort<int>()
                {
                    Array      = Array(elements),
                    Descending = new OneOfStep<bool, SortOrder>(Constant(SortOrder.Descending))
                },
                elements.OrderByDescending(x => x).ToSCLArray()
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
                new List<int>()
                {
                    0,
                    3,
                    5,
                    6,
                    7,
                    8,
                    9
                }.ToSCLArray()
            );
        }
    }
}
