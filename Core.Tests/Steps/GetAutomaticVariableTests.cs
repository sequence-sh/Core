namespace Reductech.Sequence.Core.Tests.Steps;

public partial class GetAutomaticVariableTests : StepTestBase<GetAutomaticVariable<SCLInt>, SCLInt>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Basic use",
                new Log
                {
                    Value = new ArrayElementAtIndex<SCLInt>
                    {
                        Index = Constant(0),
                        Array = new ArrayMap<SCLInt, SCLInt>
                        {
                            Array = Array(1, 2, 3),
                            Function = new LambdaFunction<SCLInt, SCLInt>(
                                null,
                                new Sum
                                {
                                    Terms = ArrayNew<SCLInt>.CreateArray(
                                        new List<IStep<SCLInt>>
                                        {
                                            new GetAutomaticVariable<SCLInt>(), Constant(13)
                                        }
                                    )
                                }
                            )
                        }
                    }
                },
                Unit.Default,
                "14"
            );

            //yield return new StepCase(
            //    "Basic use with different variable name",
            //    new Log()
            //    {
            //        Value = new ArrayElementAtIndex<SCLInt>()
            //        {
            //            Index = Constant(0),
            //            Array = new ArrayMap<SCLInt, SCLInt>()
            //            {
            //                Array = Array(1, 2, 3),
            //                Function =
            //                    new LambdaFunction<SCLInt, SCLInt>(
            //                        new VariableName("MyVar"),
            //                        new Sum()
            //                        {
            //                            Terms = ArrayNew<SCLInt>.CreateArray(
            //                                new List<IStep<SCLInt>>()
            //                                {
            //                                    new GetAutomaticVariable<SCLInt>(),
            //                                    Constant(13)
            //                                }
            //                            )
            //                        }
            //                    ),
            //            }
            //        }
            //    },
            //    Unit.Default,
            //    "14"
            //);

            yield return new StepCase(
                "Nested",
                new Log()
                {
                    Value = new ArrayElementAtIndex<SCLInt>()
                    {
                        Index = Constant(0),
                        Array = new ArrayMap<SCLInt, SCLInt>()
                        {
                            Array = Array(1, 2, 3),
                            Function = new LambdaFunction<SCLInt, SCLInt>(
                                null,
                                new Sum()
                                {
                                    Terms = ArrayNew<SCLInt>.CreateArray(
                                        new List<IStep<SCLInt>>()
                                        {
                                            new GetAutomaticVariable<SCLInt>(),
                                            new ArrayElementAtIndex<SCLInt>()
                                            {
                                                Array = new ArrayMap<SCLInt, SCLInt>()
                                                {
                                                    Array = Array(1, 2, 3),
                                                    Function = new LambdaFunction<SCLInt, SCLInt>(
                                                        null,
                                                        new Sum()
                                                        {
                                                            Terms =
                                                                ArrayNew<SCLInt>
                                                                    .CreateArray(
                                                                        new List
                                                                            <IStep<SCLInt>>()
                                                                            {
                                                                                new
                                                                                    GetAutomaticVariable
                                                                                    <SCLInt>(),
                                                                                Constant(12)
                                                                            }
                                                                    )
                                                        }
                                                    )
                                                },
                                                Index = Constant(0)
                                            }
                                        }
                                    )
                                }
                            )
                        }
                    }
                },
                Unit.Default,
                "14"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            yield return new SerializeCase(
                "Basic Case",
                new GetAutomaticVariable<SCLInt>(),
                "<>"
            );
        }
    }
}
