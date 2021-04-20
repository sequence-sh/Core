using System.Collections.Generic;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class FromJsonTests : StepTestBase<FromJson, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Single Property",
                new FromJson { Stream = Constant("{\"Foo\":1}") },
                Entity.Create(("Foo", 1))
            );

            yield return new StepCase(
                "List property",
                new FromJson { Stream = Constant(@"{""Foo"":1,""Bar"":[""a"",""b"",""c""]}") },
                Entity.Create(("Foo", 1), ("Bar", new[] { "a", "b", "c" }))
            );

            yield return new StepCase(
                "Nested Entities",
                new FromJson
                {
                    Stream = Constant(
                        @"{""Foo"":1,""Bar"":[""a"",""b"",""c""],""Baz"":{""Foo"":2,""Bar"":[""d"",""e"",""f""]}}"
                    )
                },
                Entity.Create(
                    ("Foo", 1),
                    ("Bar", new[] { "a", "b", "c" }),
                    ("Baz", Entity.Create(("Foo", 2), ("Bar", new[] { "d", "e", "f" })))
                )
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "Invalid Json",
                new FromJson { Stream = Constant("My Invalid Json") },
                ErrorCode.CouldNotParse.ToErrorBuilder("My Invalid Json", "JSON")
            );

            foreach (var ec in base.ErrorCases)
                yield return ec;
        }
    }
}

public partial class FromJsonArrayTests : StepTestBase<FromJsonArray, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            static Array<Entity> CreateArray(params Entity[] entities)
            {
                return entities.ToSCLArray();
            }

            yield return new StepCase(
                "Single Property",
                new FromJsonArray { Stream = Constant("[{\"Foo\":1}]") },
                CreateArray(Entity.Create(("Foo", 1)))
            );

            yield return new StepCase(
                "Two Entities",
                new FromJsonArray { Stream = Constant("[{\"Foo\":1},{\"Foo\":2}]") },
                CreateArray(Entity.Create(("Foo", 1)), Entity.Create(("Foo", 2)))
            );

            yield return new StepCase(
                "List property",
                new FromJsonArray
                {
                    Stream = Constant(@"[{""Foo"":1,""Bar"":[""a"",""b"",""c""]}]")
                },
                CreateArray(Entity.Create(("Foo", 1), ("Bar", new[] { "a", "b", "c" })))
            );

            yield return new StepCase(
                "Nested Entities",
                new FromJsonArray
                {
                    Stream = Constant(
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

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "Invalid Json",
                new FromJsonArray() { Stream = Constant("My Invalid Json") },
                ErrorCode.CouldNotParse.ToErrorBuilder("My Invalid Json", "JSON")
            );

            foreach (var ec in base.ErrorCases)
                yield return ec;
        }
    }
}

}
