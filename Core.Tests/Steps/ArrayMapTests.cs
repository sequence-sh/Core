namespace Reductech.EDR.Core.Tests.Steps;

public partial class ArrayMapTests : StepTestBase<ArrayMap<Entity, Entity>, Array<Entity>>
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
                    Array = new ArrayMap<Entity, Entity>
                    {
                        Array = Array(
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
                Entity.Create(("Foo", "Hello"), ("Bar", "World"))
                    .Serialize(SerializeOptions.Serialize),
                Entity.Create(("Foo", "Hello 2"), ("Bar", "World"))
                    .Serialize(SerializeOptions.Serialize)
            );

            yield return new StepCase(
                "Change property",
                new ForEach<Entity>
                {
                    Action = new LambdaFunction<Entity, Unit>(
                        null,
                        new Log<Entity> { Value = GetEntityVariable }
                    ),
                    Array = new ArrayMap<Entity, Entity>
                    {
                        Array = Array(
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
                Entity.Create(("Foo", "Hello"), ("Bar", "World"))
                    .Serialize(SerializeOptions.Serialize),
                Entity.Create(("Foo", "Hello 2"), ("Bar", "World"))
                    .Serialize(SerializeOptions.Serialize)
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
                new ArrayMap<Entity, Entity>
                {
                    Array = new FailStep<Array<Entity>> { ErrorMessage = "Stream Fail" },
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
