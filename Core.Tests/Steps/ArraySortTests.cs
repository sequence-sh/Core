namespace Sequence.Core.Tests.Steps;

public partial class ArraySortTests : StepTestBase<ArraySort<SCLInt>, Array<SCLInt>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            var elements = new[] { 8, 6, 7, 5, 3, 0, 9 };

            yield return new StepCase(
                "SortOrder False",
                new ArraySort<SCLInt>()
                {
                    Array      = Array(elements),
                    Descending = new OneOfStep<SCLBool, SCLEnum<SortOrder>>(Constant(false))
                },
                elements.OrderBy(x => x).Select(x => x.ConvertToSCLObject()).ToSCLArray()
            );

            yield return new StepCase(
                "SortOrder true",
                new ArraySort<SCLInt>()
                {
                    Array      = Array(elements),
                    Descending = new OneOfStep<SCLBool, SCLEnum<SortOrder>>(Constant(true))
                },
                elements.OrderByDescending(x => x).Select(x => x.ConvertToSCLObject()).ToSCLArray()
            );

            yield return new StepCase(
                "SortOrder Ascending",
                new ArraySort<SCLInt>()
                {
                    Array = Array(elements),
                    Descending =
                        new OneOfStep<SCLBool, SCLEnum<SortOrder>>(
                            Constant(SortOrder.Ascending)
                        )
                },
                elements.OrderBy(x => x).Select(x => x.ConvertToSCLObject()).ToSCLArray()
            );

            yield return new StepCase(
                "SortOrder Descending",
                new ArraySort<SCLInt>()
                {
                    Array = Array(elements),
                    Descending =
                        new OneOfStep<SCLBool, SCLEnum<SortOrder>>(
                            Constant(SortOrder.Descending)
                        )
                },
                elements.OrderByDescending(x => x).Select(x => x.ConvertToSCLObject()).ToSCLArray()
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
                    }.Select(x => x.ConvertToSCLObject())
                    .ToSCLArray()
            );
        }
    }
}
