using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public class ArrayConcatTests : StepTestBase<ArrayConcat<Entity>, Array<Entity>>
{
    /// <inheritdoc />
    public ArrayConcatTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "One stream",
                new ForEach<Entity>
                {
                    Action = new Print<Entity> { Value = GetEntityVariable },
                    Array = new ArrayConcat<Entity>
                    {
                        Arrays = new ArrayNew<Array<Entity>>
                        {
                            Elements = new List<IStep<Array<Entity>>>
                            {
                                Array(
                                    CreateEntity(("Foo", "Alpha")),
                                    CreateEntity(("Foo", "Beta"))
                                )
                            }
                        }
                    },
                    Variable = VariableName.Entity
                },
                Unit.Default,
                "(Foo: \"Alpha\")",
                "(Foo: \"Beta\")"
            );

            yield return new StepCase(
                "Two streams",
                new ForEach<Entity>
                {
                    Action = new Print<Entity> { Value = GetEntityVariable },
                    Array = new ArrayConcat<Entity>
                    {
                        Arrays = new ArrayNew<Array<Entity>>
                        {
                            Elements = new List<IStep<Array<Entity>>>
                            {
                                Array(
                                    CreateEntity(("Foo", "Alpha")),
                                    CreateEntity(("Foo", "Beta"))
                                ),
                                Array(
                                    CreateEntity(("Foo", "Gamma")),
                                    CreateEntity(("Foo", "Delta"))
                                )
                            }
                        }
                    },
                    Variable = VariableName.Entity
                },
                Unit.Default,
                "(Foo: \"Alpha\")",
                "(Foo: \"Beta\")",
                "(Foo: \"Gamma\")",
                "(Foo: \"Delta\")"
            );
        }
    }
}

}
