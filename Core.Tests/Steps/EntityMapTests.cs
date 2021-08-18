using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class EntityMapTests : StepTestBase<EntityMap, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Add property",
                new ForEach<Entity>
                {
                    Action = new LambdaFunction<Entity, Unit>(
                        null,
                        new Log<Entity> { Value = GetEntityVariable }
                    ),
                    Array = new EntityMap
                    {
                        EntityStream = Array(
                            Entity.Create(("Foo", "Hello")),
                            Entity.Create(("Foo", "Hello 2"))
                        ),
                        Function = new LambdaFunction<Entity, Entity>(
                            null,
                            new EntitySetValue<StringStream>
                            {
                                Entity   = GetEntityVariable,
                                Property = Constant("Bar"),
                                Value    = Constant("World")
                            }
                        )
                    }
                },
                Unit.Default,
                Entity.Create(("Foo", "Hello"),   ("Bar", "World")).Serialize(),
                Entity.Create(("Foo", "Hello 2"), ("Bar", "World")).Serialize()
            );

            yield return new StepCase(
                "Change property",
                new ForEach<Entity>
                {
                    Action = new LambdaFunction<Entity, Unit>(
                        null,
                        new Log<Entity> { Value = GetEntityVariable }
                    ),
                    Array = new EntityMap
                    {
                        EntityStream = Array(
                            Entity.Create(("Foo", "Hello"),   ("Bar", "Earth")),
                            Entity.Create(("Foo", "Hello 2"), ("Bar", "Earth"))
                        ),
                        Function = new LambdaFunction<Entity, Entity>(
                            null,
                            new EntitySetValue<StringStream>
                            {
                                Entity   = GetEntityVariable,
                                Property = Constant("Bar"),
                                Value    = Constant("World")
                            }
                        )
                    }
                },
                Unit.Default,
                Entity.Create(("Foo", "Hello"),   ("Bar", "World")).Serialize(),
                Entity.Create(("Foo", "Hello 2"), ("Bar", "World")).Serialize()
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
                "Stream error",
                new EntityMap
                {
                    EntityStream = new FailStep<Array<Entity>> { ErrorMessage = "Stream Fail" },
                    Function = new LambdaFunction<Entity, Entity>(
                        null,
                        Constant(Entity.Create(("Key", "Value")))
                    )
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
