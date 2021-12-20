namespace Reductech.Sequence.Core.Tests.Steps;

public partial class EntityRemovePropertyTests : StepTestBase<EntityRemoveProperty, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Remove property",
                new EntityRemoveProperty()
                {
                    Entity   = Constant(Entity.Create(("a", 1), ("b", 2))),
                    Property = Constant("b")
                },
                Entity.Create(("a", 1))
            );

            yield return new StepCase(
                "Remove missing property",
                new EntityRemoveProperty()
                {
                    Entity = Constant(Entity.Create(("a", 1))), Property = Constant("b")
                },
                Entity.Create(("a", 1))
            );

            yield return new StepCase(
                "Remove only property",
                new EntityRemoveProperty()
                {
                    Entity = Constant(Entity.Create(("b", 2))), Property = Constant("b")
                },
                Entity.Create()
            );

            yield return new StepCase(
                "Remove multi word property",
                new EntityRemoveProperty()
                {
                    Entity   = Constant(Entity.Create(("my property", 2))),
                    Property = Constant("my property")
                },
                Entity.Create()
            );
        }
    }
}
