namespace Reductech.EDR.Core.Tests.Steps;

public partial class ArrayFilterTests : StepTestBase<ArrayFilter<Entity>, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Filter stuff",
                new ForEach<Entity>
                {
                    Action = new LambdaFunction<Entity, Unit>(
                        null,
                        new Log { Value = GetEntityVariable }
                    ),
                    Array = new ArrayFilter<Entity>
                    {
                        Array = Array(
                            Entity.Create(("Foo", "Alpha")),
                            Entity.Create(("Bar", "Alpha")),
                            Entity.Create(("Foo", "ALPHA")),
                            Entity.Create(("Foo", "Beta")),
                            Entity.Create(("Bar", "Beta"))
                        ),
                        Predicate = new LambdaFunction<Entity, SCLBool>(
                            null,
                            new EntityHasProperty()
                            {
                                Property = Constant("Foo"), Entity = GetEntityVariable
                            }
                        )
                    }
                },
                Unit.Default,
                Entity.Create(("Foo", "Alpha")).Serialize(SerializeOptions.Serialize),
                Entity.Create(("Foo", "ALPHA")).Serialize(SerializeOptions.Serialize),
                Entity.Create(("Foo", "Beta")).Serialize(SerializeOptions.Serialize)
            );

            yield return new StepCase(
                "Filter stuff with custom variable name",
                new ForEach<Entity>
                {
                    Action = new LambdaFunction<Entity, Unit>(
                        new VariableName("ForeachVar"),
                        new Log { Value = GetVariable<Entity>("ForeachVar") }
                    ),
                    Array = new ArrayFilter<Entity>
                    {
                        Array = Array(
                            Entity.Create(("Foo", "Alpha")),
                            Entity.Create(("Bar", "Alpha")),
                            Entity.Create(("Foo", "ALPHA")),
                            Entity.Create(("Foo", "Beta")),
                            Entity.Create(("Bar", "Beta"))
                        ),
                        Predicate = new LambdaFunction<Entity, SCLBool>(
                            new VariableName("FilterVar"),
                            new EntityHasProperty()
                            {
                                Property = Constant("Foo"),
                                Entity   = GetVariable<Entity>("FilterVar")
                            }
                        )
                    }
                },
                Unit.Default,
                Entity.Create(("Foo", "Alpha")).Serialize(SerializeOptions.Serialize),
                Entity.Create(("Foo", "ALPHA")).Serialize(SerializeOptions.Serialize),
                Entity.Create(("Foo", "Beta")).Serialize(SerializeOptions.Serialize)
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
                    Predicate = new LambdaFunction<Entity, SCLBool>(null, Constant(true))
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
