namespace Reductech.Sequence.Core.Tests.Steps;

public partial class ArraySortEntitiesTests : StepTestBase<ArraySort<Entity>, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Ascending",
                new ForEach<Entity>
                {
                    Array = new ArraySort<Entity>
                    {
                        Array = Array(
                            Entity.Create(("Foo", "Gamma")),
                            Entity.Create(("Foo", "Alpha")),
                            Entity.Create(("Foo", "Beta"))
                        ),
                        KeySelector = new LambdaFunction<Entity, StringStream>(
                            null,
                            new EntityGetValue<StringStream>
                            {
                                Entity = GetEntityVariable, Property = Constant("Foo")
                            }
                        )
                    },
                    Action = new LambdaFunction<Entity, Unit>(
                        null,
                        new Log { Value = GetEntityVariable }
                    ),
                },
                Unit.Default,
                Entity.Create(("Foo", "Alpha")).Serialize(SerializeOptions.Serialize),
                Entity.Create(("Foo", "Beta")).Serialize(SerializeOptions.Serialize),
                Entity.Create(("Foo", "Gamma")).Serialize(SerializeOptions.Serialize)
            );

            yield return new StepCase(
                "Descending",
                new ForEach<Entity>
                {
                    Array = new ArraySort<Entity>
                    {
                        Descending = new OneOfStep<SCLBool, SCLEnum<SortOrder>>(Constant(true)),
                        Array = Array(
                            Entity.Create(("Foo", "Gamma")),
                            Entity.Create(("Foo", "Alpha")),
                            Entity.Create(("Foo", "Beta"))
                        ),
                        KeySelector = new LambdaFunction<Entity, StringStream>(
                            null,
                            new EntityGetValue<StringStream>
                            {
                                Entity = GetEntityVariable, Property = Constant("Foo")
                            }
                        )
                    },
                    Action = new LambdaFunction<Entity, Unit>(
                        null,
                        new Log { Value = GetEntityVariable }
                    )
                },
                Unit.Default,
                Entity.Create(("Foo", "Gamma")).Serialize(SerializeOptions.Serialize),
                Entity.Create(("Foo", "Beta")).Serialize(SerializeOptions.Serialize),
                Entity.Create(("Foo", "Alpha")).Serialize(SerializeOptions.Serialize)
            );

            yield return new StepCase(
                "Missing Property",
                new ForEach<Entity>
                {
                    Array = new ArraySort<Entity>
                    {
                        Descending = new OneOfStep<SCLBool, SCLEnum<SortOrder>>(Constant(true)),
                        Array = Array(
                            Entity.Create(("Foo", "Gamma")),
                            Entity.Create(("Foo", "Alpha")),
                            Entity.Create(("Foo", "Beta")),
                            Entity.Create(("Bar", "Delta"))
                        ),
                        KeySelector = new LambdaFunction<Entity, StringStream>(
                            null,
                            new EntityGetValue<StringStream>
                            {
                                Entity = GetEntityVariable, Property = Constant("Foo")
                            }
                        )
                    },
                    Action = new LambdaFunction<Entity, Unit>(
                        null,
                        new Log { Value = GetEntityVariable }
                    ),
                },
                Unit.Default,
                Entity.Create(("Foo", "Gamma")).Serialize(SerializeOptions.Serialize),
                Entity.Create(("Foo", "Beta")).Serialize(SerializeOptions.Serialize),
                Entity.Create(("Foo", "Alpha")).Serialize(SerializeOptions.Serialize),
                Entity.Create(("Bar", "Delta")).Serialize(SerializeOptions.Serialize)
            );
        }
    }
}
