using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public class ArrayFilterTests : StepTestBase<ArrayFilter<Entity>, Array<Entity>>
{
    /// <inheritdoc />
    public ArrayFilterTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Filter stuff",
                new ForEach<Entity>
                {
                    Action = new Print<Entity> { Value = GetEntityVariable },
                    Array = new ArrayFilter<Entity>
                    {
                        Array = Array(
                            CreateEntity(("Foo", "Alpha")),
                            CreateEntity(("Bar", "Alpha")),
                            CreateEntity(("Foo", "ALPHA")),
                            CreateEntity(("Foo", "Beta")),
                            CreateEntity(("Bar", "Beta"))
                        ),
                        Predicate = new EntityHasProperty()
                        {
                            Property = Constant("Foo"), Entity = GetEntityVariable
                        },
                    },
                    Variable = VariableName.Entity
                },
                Unit.Default,
                "(Foo: \"Alpha\")",
                "(Foo: \"ALPHA\")",
                "(Foo: \"Beta\")"
            );

            yield return new StepCase(
                "Filter stuff with custom variable name",
                new ForEach<Entity>
                {
                    Action   = new Print<Entity> { Value = GetVariable<Entity>("ForeachVar") },
                    Variable = new VariableName("ForeachVar"),
                    Array = new ArrayFilter<Entity>
                    {
                        Array = Array(
                            CreateEntity(("Foo", "Alpha")),
                            CreateEntity(("Bar", "Alpha")),
                            CreateEntity(("Foo", "ALPHA")),
                            CreateEntity(("Foo", "Beta")),
                            CreateEntity(("Bar", "Beta"))
                        ),
                        Predicate =
                            new EntityHasProperty()
                            {
                                Property = Constant("Foo"),
                                Entity   = GetVariable<Entity>("FilterVar")
                            },
                        Variable = new VariableName("FilterVar")
                    }
                },
                Unit.Default,
                "(Foo: \"Alpha\")",
                "(Foo: \"ALPHA\")",
                "(Foo: \"Beta\")"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            //Do not do default cases as some errors are not propagated due to lazy evaluation

            yield return new ErrorCase(
                "EntityStream is error",
                new ArrayFilter<Entity>()
                {
                    Array     = new FailStep<Array<Entity>>() { ErrorMessage = "Stream Fail" },
                    Predicate = Constant(true)
                },
                new SingleError(
                    EntireSequenceLocation.Instance,
                    ErrorCode.Test,
                    "Stream Fail"
                )
            );
        }
    }
}

}
