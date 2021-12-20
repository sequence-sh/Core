namespace Reductech.Sequence.Core.Tests.Steps;

public partial class ArrayGroupByIntTests : StepTestBase<ArrayGroupBy<SCLInt>, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Group ints by Value",
                new ArrayGroupBy<SCLInt>()
                {
                    Array = Array(1, 2, 1, 2, 3),
                    Function = new LambdaFunction<SCLInt, StringStream>(
                        null,
                        new StringInterpolate()
                        {
                            Strings = new[] { GetVariable<StringStream>(VariableName.Item) }
                        }
                    )
                },
                new EagerArray<Entity>(
                    new List<Entity>()
                    {
                        Entity.Create(("Key", "1"), ("Values", new List<int>() { 1, 1 })),
                        Entity.Create(("Key", "2"), ("Values", new List<int>() { 2, 2 })),
                        Entity.Create(("Key", "3"), ("Values", new List<int>() { 3 })),
                    }
                ).Select(x => x)
            );
        }
    }
}

public partial class ArrayGroupByEntityTests : StepTestBase<ArrayGroupBy<Entity>, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Group ints by Value",
                new ArrayGroupBy<Entity>()
                {
                    Array = Array(
                        Entity.Create(("Val", 1)),
                        Entity.Create(("Val", 1)),
                        Entity.Create(("Val", 2)),
                        Entity.Create(("Val", 2)),
                        Entity.Create(("Val", 3))
                    ),
                    Function = new LambdaFunction<Entity, StringStream>(
                        null,
                        new EntityGetValue<StringStream>()
                        {
                            Entity = GetEntityVariable, Property = Constant("Val")
                        }
                    )
                },
                new EagerArray<Entity>(
                    new List<Entity>()
                    {
                        Entity.Create(
                            ("Key", "1"),
                            ("Values",
                             new List<Entity>()
                             {
                                 Entity.Create(("Val", 1)), Entity.Create(("Val", 1))
                             })
                        ),
                        Entity.Create(
                            ("Key", "2"),
                            ("Values",
                             new List<Entity>()
                             {
                                 Entity.Create(("Val", 2)), Entity.Create(("Val", 2))
                             })
                        ),
                        Entity.Create(
                            ("Key", "3"),
                            ("Values", new List<Entity>() { Entity.Create(("Val", 3)) })
                        ),
                    }
                ).Select(x => x)
            );
        }
    }
}
