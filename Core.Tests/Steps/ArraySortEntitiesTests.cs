using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

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
                        KeySelector = new EntityGetValue<StringStream>
                        {
                            Entity = GetEntityVariable, Property = Constant("Foo")
                        }
                    },
                    Action   = new Log<Entity> { Value = GetEntityVariable },
                    Variable = VariableName.Entity
                },
                Unit.Default,
                "(Foo: \"Alpha\")",
                "(Foo: \"Beta\")",
                "(Foo: \"Gamma\")"
            );

            yield return new StepCase(
                "Descending",
                new ForEach<Entity>
                {
                    Array = new ArraySort<Entity>
                    {
                        Descending = Constant(true),
                        Array = Array(
                            Entity.Create(("Foo", "Gamma")),
                            Entity.Create(("Foo", "Alpha")),
                            Entity.Create(("Foo", "Beta"))
                        ),
                        KeySelector = new EntityGetValue<StringStream>
                        {
                            Entity = GetEntityVariable, Property = Constant("Foo")
                        }
                    },
                    Action   = new Log<Entity> { Value = GetEntityVariable },
                    Variable = VariableName.Entity
                },
                Unit.Default,
                "(Foo: \"Gamma\")",
                "(Foo: \"Beta\")",
                "(Foo: \"Alpha\")"
            );

            yield return new StepCase(
                "Missing Property",
                new ForEach<Entity>
                {
                    Array = new ArraySort<Entity>
                    {
                        Descending = Constant(true),
                        Array = Array(
                            Entity.Create(("Foo", "Gamma")),
                            Entity.Create(("Foo", "Alpha")),
                            Entity.Create(("Foo", "Beta")),
                            Entity.Create(("Bar", "Delta"))
                        ),
                        KeySelector = new EntityGetValue<StringStream>
                        {
                            Entity = GetEntityVariable, Property = Constant("Foo")
                        }
                    },
                    Action   = new Log<Entity> { Value = GetEntityVariable },
                    Variable = VariableName.Entity
                },
                Unit.Default,
                "(Foo: \"Gamma\")",
                "(Foo: \"Beta\")",
                "(Foo: \"Alpha\")",
                "(Bar: \"Delta\")"
            );
        }
    }
}

}
