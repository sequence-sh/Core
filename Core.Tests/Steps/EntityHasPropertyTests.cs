namespace Reductech.EDR.Core.Tests.Steps;

public partial class EntityHasPropertyTests : StepTestBase<EntityHasProperty, SCLBool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Property exists",
                new EntityHasProperty
                {
                    Property = Constant("Foo"),
                    Entity   = Constant(Entity.Create(("Foo", "Hello")))
                },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Property missing",
                new EntityHasProperty
                {
                    Property = Constant("Bar"),
                    Entity   = Constant(Entity.Create(("Hello", "World")))
                },
                false.ConvertToSCLObject()
            );
        }
    }
}
