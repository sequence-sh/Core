using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ToJsonTests : StepTestBase<ToJson, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Single Property",
                new ToJson()
                {
                    Entity = Constant(Entity.Create(("Foo", 1))), FormatOutput = Constant(false)
                },
                "{\"Foo\":1}"
            );

            yield return new StepCase(
                "Single Property Formatted",
                new ToJson { Entity = Constant(Entity.Create(("Foo", 1))) },
                "{\r\n\"Foo\": 1\r\n}"
            );

            yield return new StepCase(
                "List property",
                new ToJson()
                {
                    Entity = Constant(
                        Entity.Create(("Foo", 1), ("Bar", new[] { "a", "b", "c" }))
                    ),
                    FormatOutput = Constant(false)
                },
                @"{""Foo"":1,""Bar"":[""a"",""b"",""c""]}"
            );

            yield return new StepCase(
                "Nested Entities",
                new ToJson()
                {
                    Entity = Constant(
                        Entity.Create(
                            ("Foo", 1),
                            ("Bar", new[] { "a", "b", "c" }),
                            ("Baz", Entity.Create(("Foo", 2), ("Bar", new[] { "d", "e", "f" })))
                        )
                    ),
                    FormatOutput = Constant(false)
                },
                @"{""Foo"":1,""Bar"":[""a"",""b"",""c""],""Baz"":{""Foo"":2,""Bar"":[""d"",""e"",""f""]}}"
            );
        }
    }
}

}
