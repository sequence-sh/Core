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
                    Action = new LambdaFunction<Entity, Unit>(
                        foreachVar,
                        new Log<Entity> { Value = GetVariable<Entity>(foreachVar) }
                    ),
                    Array = new ArrayDistinct<Entity>
                    {
                        Array = Array(
                            Entity.Create(("Foo", "Alpha")),
                            Entity.Create(("Foo", "Alpha")),
                            Entity.Create(("Foo", "ALPHA")),
                            Entity.Create(("Foo", "Beta")),
                            Entity.Create(("Foo", "Beta"))
                        ),
                        KeySelector = new LambdaFunction<Entity, StringStream>(
                            distinctVar,
                            new EntityGetValue<StringStream>
                            {
                                Property = Constant("Foo"),
                                Entity   = GetVariable<Entity>(distinctVar)
                            }
                        )
                    },
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
                    Action = new LambdaFunction<Entity, Unit>(
                        VariableName.Item,
                        new Log<Entity> { Value = GetEntityVariable }
                    ),
                    Array = new ArrayDistinct<Entity>
                    {
                        Array = Array(
                            Entity.Create(("Foo", "Alpha")),
                            Entity.Create(("Foo", "Alpha")),
                            Entity.Create(("Foo", "ALPHA")),
                            Entity.Create(("Foo", "Beta")),
                            Entity.Create(("Foo", "Beta"))
                        ),
                        KeySelector = new LambdaFunction<Entity, StringStream>(
                            VariableName.Item,
                            new EntityGetValue<StringStream>
                            {
                                Property = Constant("Foo"), Entity = GetEntityVariable
                            }
                        ),
                        IgnoreCase = Constant(true)
                    },
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
                    KeySelector = new LambdaFunction<Entity, StringStream>(null, Constant("A"))
                },
                new SingleError(
                    ErrorLocation.EmptyLocation,
                    ErrorCode.Test,
                    "Stream Fail"
                )
            );
        }
    }
}

}
