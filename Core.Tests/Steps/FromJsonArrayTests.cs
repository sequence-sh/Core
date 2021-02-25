using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class FromJsonTests : StepTestBase<FromJsonArray, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            static Array<Entity> CreateArray(params Entity[] entities)
            {
                return new(entities);
            }

            yield return new StepCase(
                "Single Property",
                new FromJsonArray { Stream = StaticHelpers.Constant("[{\"Foo\":1}]") },
                new Array<Entity>(new List<Entity>() { Entity.Create(("Foo", 1)) })
            );

            yield return new StepCase(
                "Two Entities",
                new FromJsonArray { Stream = StaticHelpers.Constant("[{\"Foo\":1},{\"Foo\":2}]") },
                CreateArray(Entity.Create(("Foo", 1)), Entity.Create(("Foo", 2)))
            );

            yield return new StepCase(
                "List property",
                new FromJsonArray
                {
                    Stream = StaticHelpers.Constant(
                        @"[{""Foo"":1,""Bar"":[""a"",""b"",""c""]}]"
                    )
                },
                CreateArray(Entity.Create(("Foo", 1), ("Bar", new[] { "a", "b", "c" })))
            );

            yield return new StepCase(
                "Nested Entities",
                new FromJsonArray
                {
                    Stream = StaticHelpers.Constant(
                        @"[{""Foo"":1,""Bar"":[""a"",""b"",""c""],""Baz"":{""Foo"":2,""Bar"":[""d"",""e"",""f""]}}]"
                    )
                },
                CreateArray(
                    Entity.Create(
                        ("Foo", 1),
                        ("Bar", new[] { "a", "b", "c" }),
                        ("Baz", Entity.Create(("Foo", 2), ("Bar", new[] { "d", "e", "f" })))
                    )
                )
            );
        }
    }
}

}
