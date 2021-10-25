﻿using System.Collections.Generic;
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
                            Mappings = Constant(Entity.Create(("Foo", "Food")))
                        }
                },
                Unit.Default,
                Entity.Create(("Foo", "Hello"),   ("Bar", "World")).Serialize(),
                Entity.Create(("Foo", "Hello 2"), ("Bar", "World 2")).Serialize()
            );

            yield return new StepCase(
                "Map with field list",
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
                                    ("Fish", "Hello"),
                                    ("Bar", "World")
                                ),
                                Entity.Create(
                                    ("Meat", "Hello 2"),
                                    ("Bar", "World 2")
                                )
                            ),
                            Mappings = Constant(Entity.Create(("Foo", new[] { "Fish", "Meat" })))
                        }
                },
                Unit.Default,
                Entity.Create(("Foo", "Hello"),   ("Bar", "World")).Serialize(),
                Entity.Create(("Foo", "Hello 2"), ("Bar", "World 2")).Serialize()
            );

            yield return new StepCase(
                "Map nested property",
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
                                    ("Food", Entity.Create(("Fish", "Hello"))),
                                    ("Bar", "World")
                                ),
                                Entity.Create(
                                    ("Food", Entity.Create(("Meat", "Hello 2"))),
                                    ("Bar", "World 2")
                                )
                            ),
                            Mappings = Constant(
                                Entity.Create(("Foo", new[] { "Food.Fish", "Food.Meat" }))
                            )
                        }
                },
                Unit.Default,
                Entity.Create(("Foo", "Hello"), ("Bar", "World"))
                    .Serialize(),
                Entity.Create(("Foo", "Hello 2"), ("Bar", "World 2"))
                    .Serialize()
            );

            yield return new StepCase(
                "Map nested property with missing property",
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
                                    ("Food", Entity.Create(("Fish", "Hello"))),
                                    ("Bar", "World")
                                ),
                                Entity.Create(
                                    ("Food", Entity.Create(("Meat", "Hello 2"))),
                                    ("Bar", "World 2")
                                ),
                                Entity.Create(
                                    ("Food", Entity.Create(("Something", "Hello 3"))),
                                    ("Bar", "World 3")
                                ),
                                Entity.Create(("Bar", "World 4"))
                            ),
                            Mappings = Constant(
                                Entity.Create(("Foo", new[] { "Food.Fish", "Food.Meat" }))
                            )
                        }
                },
                Unit.Default,
                Entity.Create(("Foo", "Hello"), ("Bar", "World"))
                    .Serialize(),
                Entity.Create(("Foo", "Hello 2"), ("Bar", "World 2"))
                    .Serialize(),
                Entity.Create(("Food", Entity.Create(("Something", "Hello 3"))), ("Bar", "World 3"))
                    .Serialize(),
                Entity.Create(("Bar", "World 4")).Serialize()
            );
        }
    }
}

}
