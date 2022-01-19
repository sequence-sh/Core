namespace Reductech.Sequence.Core.Tests.Steps;

public partial class ForEachTests : StepTestBase<ForEach<SCLInt>, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Default Variable Name",
                new ForEach<SCLInt>
                {
                    Action = new LambdaFunction<SCLInt, Unit>(
                        null,
                        new Log { Value = GetVariable<SCLInt>(VariableName.Item) }
                    ),
                    Array = Array(3, 2, 1)
                },
                Unit.Default,
                "3",
                "2",
                "1"
            );

            yield return new StepCase(
                "Custom Variable Name",
                new ForEach<SCLInt>
                {
                    Action = new LambdaFunction<SCLInt, Unit>(
                        new VariableName("Foo"),
                        new Log { Value = GetVariable<SCLInt>("Foo") }
                    ),
                    Array = Array(3, 2, 1),
                },
                Unit.Default,
                "3",
                "2",
                "1"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Default Variable Name",
                "Foreach [3,2,1]  (Log Value: <item>)",
                Unit.Default,
                "3",
                "2",
                "1"
            );

            yield return new DeserializeCase(
                "Named Variable",
                "Foreach [3,2,1] (<Foo> => Log Value: <Foo>)",
                Unit.Default,
                "3",
                "2",
                "1"
            );

            yield return new DeserializeCase(
                "Scoped Variable Overloading",
                @"- Foreach [1,2,3] (Log <item>) #Here <item> is an int
- Foreach ['one', 'two','three'] (Log <item>) #Here <item> is a string
",
                Unit.Default,
                "1",
                "2",
                "3",
                "one",
                "two",
                "three"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "Array Failure",
                new ForEach<SCLInt>
                {
                    Array = new FailStep<Array<SCLInt>> { ErrorMessage = "Array Failure" }
                },
                new SingleError(
                    ErrorLocation.EmptyLocation,
                    ErrorCode.Test,
                    "Array Failure"
                )
            );

            yield return new ErrorCase(
                "Action Failure",
                new ForEach<SCLInt>
                {
                    Array = Array(1),
                    Action = new LambdaFunction<SCLInt, Unit>(
                        null,
                        new FailStep<Unit> { ErrorMessage = "Action Failure" }
                    ),
                },
                new SingleError(
                    ErrorLocation.EmptyLocation,
                    ErrorCode.Test,
                    "Action Failure"
                )
            );
        }
    }
}
