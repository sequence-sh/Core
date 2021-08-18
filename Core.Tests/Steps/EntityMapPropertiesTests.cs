using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class EntityMapPropertiesTests : StepTestBase<EntityMapProperties, Array<Entity>>
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
                    Action = new LambdaFunction<Entity, Unit>(
                        null,
                        new Log<Entity> { Value = GetEntityVariable }
                    ),
                    Array =
                        new EntityMapProperties
                        {
                            EntityStream = Array(
                                Entity.Create(
                                    ("Food", "Hello"),
                                    ("Bar", "World")
                                ),
                                Entity.Create(
                                    ("Food", "Hello 2"),
                                    ("Bar", "World 2")
                                )
                            ),
                            Mappings = Constant(Entity.Create(("Food", "Foo")))
                        }
                },
                Unit.Default,
                Entity.Create(("Foo", "Hello"),   ("Bar", "World")).Serialize(),
                Entity.Create(("Foo", "Hello 2"), ("Bar", "World 2")).Serialize()
            );
        }
    }
}

}
