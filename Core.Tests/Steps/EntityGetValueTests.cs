using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class EntityGetValueTests : StepTestBase<EntityGetValue, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Get Simple Property",
                new EntityGetValue
                {
                    Entity   = Constant(Entity.Create(("Foo", "Hello"), ("Bar", "World"))),
                    Property = Constant("Foo")
                },
                "Hello"
            );

            yield return new StepCase(
                "Get Missing Property",
                new EntityGetValue
                {
                    Entity   = Constant(Entity.Create(("Foo", "Hello"), ("Bar", "World"))),
                    Property = Constant("Foot")
                },
                ""
            );

            yield return new StepCase(
                "Get Empty Property",
                new EntityGetValue
                {
                    Entity   = Constant(Entity.Create(("Foo", ""), ("Bar", "World"))),
                    Property = Constant("Foo")
                },
                ""
            );

            yield return new StepCase(
                "Get List Property",
                new EntityGetValue
                {
                    Entity   = Constant(Entity.Create(("Foo", new[] { "Hello", "World" }))),
                    Property = Constant("Foo")
                },
                "Hello,World"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            var (step, _) = CreateStepWithDefaultOrArbitraryValues();

            yield return new SerializeCase(
                "Default",
                step,
                "(Prop1: \"Val0\" Prop2: \"Val1\")[\"Bar2\"]"
            );
        }
    }
}

}
