namespace Sequence.Core.Tests.Steps;

public partial class ForTests : StepTestBase<For, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Increment 1",
                new For
                {
                    Action = new LambdaFunction<SCLInt, Unit>(
                        null,
                        new Log { Value = GetVariable<SCLInt>(VariableName.Item) }
                    ),
                    From      = Constant(1),
                    To        = Constant(3),
                    Increment = Constant(1),
                },
                Unit.Default,
                "1",
                "2",
                "3"
            );

            yield return new StepCase(
                "Increment 2",
                new For
                {
                    Action = new LambdaFunction<SCLInt, Unit>(
                        null,
                        new Log { Value = GetVariable<SCLInt>(VariableName.Item) }
                    ),
                    From      = Constant(1),
                    To        = Constant(6),
                    Increment = Constant(2),
                },
                Unit.Default,
                "1",
                "3",
                "5"
            );

            yield return new StepCase(
                "Increment -1",
                new For
                {
                    Action = new LambdaFunction<SCLInt, Unit>(
                        null,
                        new Log { Value = GetVariable<SCLInt>(VariableName.Item) }
                    ),
                    From      = Constant(3),
                    To        = Constant(1),
                    Increment = Constant(-1),
                },
                Unit.Default,
                "3",
                "2",
                "1"
            );

            yield return new StepCase(
                "Increment No range",
                new For
                {
                    Action = new LambdaFunction<SCLInt, Unit>(
                        null,
                        new Log { Value = GetVariable<SCLInt>(VariableName.Item) }
                    ),
                    From      = Constant(3),
                    To        = Constant(1),
                    Increment = Constant(1),
                },
                Unit.Default
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Increment 1",
                "For Action: (Log Value: <item>) From: 1 To: 3 Increment: 1",
                Unit.Default,
                "1",
                "2",
                "3"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                    "ValueIf increment 0",
                    new For
                    {
                        Action = new LambdaFunction<SCLInt, Unit>(
                            null,
                            new Log { Value = GetVariable<SCLInt>(VariableName.Item) }
                        ),
                        From      = Constant(1),
                        To        = Constant(3),
                        Increment = Constant(0)
                    },
                    new ErrorBuilder(ErrorCode.DivideByZero)
                )
                ;

            foreach (var errorCase in base.ErrorCases)
                yield return errorCase;
        }
    }
}
