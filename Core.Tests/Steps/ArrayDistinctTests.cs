using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ArrayDistinctTests : StepTestBase<ArrayDistinct<Entity>, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            var distinctVar = new VariableName("DistinctVar");
            var foreachVar  = new VariableName("ForeachVar");

            yield return new StepCase(
                "Distinct case sensitive",
                new ForEach<Entity>
                {
                    Action = new Log<Entity> { Value = GetVariable<Entity>(foreachVar) },
                    Array = new ArrayDistinct<Entity>
                    {
                        Array = Array(
                            CreateEntity(("Foo", "Alpha")),
                            CreateEntity(("Foo", "Alpha")),
                            CreateEntity(("Foo", "ALPHA")),
                            CreateEntity(("Foo", "Beta")),
                            CreateEntity(("Foo", "Beta"))
                        ),
                        KeySelector = new EntityGetValue
                        {
                            Property = Constant("Foo"),
                            Entity   = GetVariable<Entity>(distinctVar)
                        },
                        Variable = distinctVar
                    },
                    Variable = foreachVar
                },
                Unit.Default,
                "(Foo: \"Alpha\")",
                "(Foo: \"ALPHA\")",
                "(Foo: \"Beta\")"
            );

            yield return new StepCase(
                "Distinct case insensitive",
                new ForEach<Entity>
                {
                    Action = new Log<Entity> { Value = GetEntityVariable },
                    Array = new ArrayDistinct<Entity>
                    {
                        Array = Array(
                            CreateEntity(("Foo", "Alpha")),
                            CreateEntity(("Foo", "Alpha")),
                            CreateEntity(("Foo", "ALPHA")),
                            CreateEntity(("Foo", "Beta")),
                            CreateEntity(("Foo", "Beta"))
                        ),
                        KeySelector =
                            new EntityGetValue
                            {
                                Property = Constant("Foo"), Entity = GetEntityVariable
                            },
                        IgnoreCase = Constant(true)
                    },
                    Variable = VariableName.Entity
                },
                Unit.Default,
                "(Foo: \"Alpha\")",
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
                "Stream is error",
                new ArrayDistinct<Entity>
                {
                    Array       = new FailStep<Array<Entity>> { ErrorMessage = "Stream Fail" },
                    KeySelector = Constant("A")
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
