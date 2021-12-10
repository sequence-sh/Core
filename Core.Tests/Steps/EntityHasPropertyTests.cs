using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class EntityHasPropertyTests : StepTestBase<EntityHasProperty, bool>
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
                true
            );

            yield return new StepCase(
                "Property missing",
                new EntityHasProperty
                {
                    Property = Constant("Bar"),
                    Entity   = Constant(Entity.Create(("Hello", "World")))
                },
                false
            );
        }
    }
}
