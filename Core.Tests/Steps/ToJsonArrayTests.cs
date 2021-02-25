using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ToJsonArrayTests : StepTestBase<ToJsonArray, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Single Property",
                new ToJsonArray() { Entities = Array(Entity.Create(("Foo", 1))) },
                "[{\"Foo\":1}]"
            );

            yield return new StepCase(
                "Two Entities",
                new ToJsonArray()
                {
                    Entities = Array(Entity.Create(("Foo", 1)), Entity.Create(("Foo", 2)))
                },
                "[{\"Foo\":1},{\"Foo\":2}]"
            );

            yield return new StepCase(
                "List property",
                new ToJsonArray()
                {
                    Entities = Array(
                        Entity.Create(("Foo", 1), ("Bar", new[] { "a", "b", "c" }))
                    )
                },
                @"[{""Foo"":1,""Bar"":[""a"",""b"",""c""]}]"
            );

            yield return new StepCase(
                "Nested Entities",
                new ToJsonArray()
                {
                    Entities = Array(
                        Entity.Create(
                            ("Foo", 1),
                            ("Bar", new[] { "a", "b", "c" }),
                            ("Baz", Entity.Create(("Foo", 2), ("Bar", new[] { "d", "e", "f" })))
                        )
                    )
                },
                @"[{""Foo"":1,""Bar"":[""a"",""b"",""c""],""Baz"":{""Foo"":2,""Bar"":[""d"",""e"",""f""]}}]"
            );
        }
    }
}

}
