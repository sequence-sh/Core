namespace Sequence.Core.Tests.Steps;

public partial class
    EntityValidateRelationsTests : StepTestBase<EntityValidateRelations, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Check stream can still be read after skip",
                new ForEach<Entity>()
                {
                    Array = new EntityValidateRelations()
                    {
                        EntityStream = Array(
                            Entity.Create(("Foo", 1), ("Bar", 1)),
                            Entity.Create(("Foo", 2), ("Bar", 100))
                        ),
                        IdProperty       = Constant("Foo"),
                        ParentIdProperty = Constant("Bar"),
                        ErrorBehavior    = Constant(ErrorBehavior.Skip)
                    },
                    Action = new LambdaFunction<Entity, Unit>(
                        VariableName.Item,
                        new Log() { Value = GetEntityVariable }
                    )
                },
                Unit.Default,
                "('Foo': 1 'Bar': 1)"
            );

            yield return new StepCase(
                "Check stream can still be read after no change",
                new ForEach<Entity>()
                {
                    Array = new EntityValidateRelations()
                    {
                        EntityStream     = Array(Entity.Create(("Foo", 1), ("Bar", 1))),
                        IdProperty       = Constant("Foo"),
                        ParentIdProperty = Constant("Bar"),
                        ErrorBehavior    = Constant(ErrorBehavior.Skip)
                    },
                    Action = new LambdaFunction<Entity, Unit>(
                        VariableName.Item,
                        new Log() { Value = GetEntityVariable }
                    )
                },
                Unit.Default,
                "('Foo': 1 'Bar': 1)"
            );

            yield break;
        }
    }
}
