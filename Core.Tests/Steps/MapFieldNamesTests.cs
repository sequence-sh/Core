using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class MapFieldNamesTests : StepTestBase<EntityMapProperties, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Map some fields",
                new ForEach<Entity>
                {
                    Action = new Print<Entity> { Value = GetVariable<Entity>(VariableName.Entity) },
                    Array =
                        new EntityMapProperties
                        {
                            EntityStream = Array(
                                CreateEntity(
                                    ("Food", "Hello"),
                                    ("Bar", "World")
                                ),
                                CreateEntity(
                                    ("Food", "Hello 2"),
                                    ("Bar", "World 2")
                                )
                            ),
                            Mappings = Constant(CreateEntity(("Food", "Foo")))
                        },
                    Variable = VariableName.Entity
                },
                Unit.Default,
                "(Foo: \"Hello\" Bar: \"World\")",
                "(Foo: \"Hello 2\" Bar: \"World 2\")"
            );
        }
    }
}

}
