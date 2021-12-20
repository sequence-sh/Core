namespace Reductech.EDR.Core.Tests.Steps;

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
                        new Log { Value = GetVariable<Entity>(foreachVar) }
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
                Entity.Create(("Foo", "Alpha")).Serialize(SerializeOptions.Serialize),
                Entity.Create(("Foo", "ALPHA")).Serialize(SerializeOptions.Serialize),
                Entity.Create(("Foo", "Beta")).Serialize(SerializeOptions.Serialize)
            );

            yield return new StepCase(
                "Distinct case insensitive",
                new ForEach<Entity>
                {
                    Action = new LambdaFunction<Entity, Unit>(
                        VariableName.Item,
                        new Log { Value = GetEntityVariable }
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
                Entity.Create(("Foo", "Alpha")).Serialize(SerializeOptions.Serialize),
                Entity.Create(("Foo", "Beta")).Serialize(SerializeOptions.Serialize)
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases => base.SerializeCases.Select(
        x => x with
        {
            ExpectedSCL =
            @"ArrayDistinct Array: [('Prop1': ""Val0"" 'Prop2': ""Val1""), ('Prop1': ""Val2"" 'Prop2': ""Val3""), ('Prop1': ""Val4"" 'Prop2': ""Val5"")] KeySelector: (<> => $""{<>}"") IgnoreCase: False"
        }
    );

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
