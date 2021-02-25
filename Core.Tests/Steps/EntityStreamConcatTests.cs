using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ArrayConcatTests : StepTestBase<ArrayConcat<Entity>, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "One stream",
                new ForEach<Entity>
                {
                    Action = new Log<Entity> { Value = GetEntityVariable },
                    Array = new ArrayConcat<Entity>
                    {
                        Arrays = new ArrayNew<Array<Entity>>
                        {
                            Elements = new List<IStep<Array<Entity>>>
                            {
                                Array(
                                    Entity.Create(("Foo", "Alpha")),
                                    Entity.Create(("Foo", "Beta"))
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
                    Action = new Log<Entity> { Value = GetEntityVariable },
                    Array = new ArrayConcat<Entity>
                    {
                        Arrays = new ArrayNew<Array<Entity>>
                        {
                            Elements = new List<IStep<Array<Entity>>>
                            {
                                Array(
                                    Entity.Create(("Foo", "Alpha")),
                                    Entity.Create(("Foo", "Beta"))
                                ),
                                Array(
                                    Entity.Create(("Foo", "Gamma")),
                                    Entity.Create(("Foo", "Delta"))
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
