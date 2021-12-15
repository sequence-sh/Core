namespace Reductech.EDR.Core.Tests.Steps;

public partial class IfTests : StepTestBase<If<Unit>, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "If true Log something",
                "If Condition: true Then: (Log Value: 'Hello World')",
                Unit.Default,
                "Hello World"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "If true Log something",
                new If<Unit>
                {
                    Condition = Constant(true),
                    Then      = new Log() { Value = Constant("Hello World") }
                },
                Unit.Default,
                "Hello World"
            );

            yield return new StepCase(
                "If false Log nothing",
                new If<Unit>
                {
                    Condition = Constant(false),
                    Then      = new Log { Value = Constant("Hello World") }
                },
                Unit.Default
            );

            yield return new StepCase(
                "If false Log something else",
                new If<Unit>
                {
                    Condition = Constant(false),
                    Then      = new Log { Value = Constant("Hello World") },
                    Else      = new Log { Value = Constant("Goodbye World") },
                },
                Unit.Default,
                "Goodbye World"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "Condition is error",
                new If<Unit>()
                {
                    Condition = new FailStep<SCLBool> { ErrorMessage = "Condition Fail" },
                    Then      = new FailStep<Unit> { ErrorMessage    = "Then Fail" },
                    Else      = new FailStep<Unit> { ErrorMessage    = "Else Fail" },
                },
                new SingleError(
                    ErrorLocation.EmptyLocation,
                    ErrorCode.Test,
                    "Condition Fail"
                )
            );

            yield return new ErrorCase(
                "Then is error",
                new If<Unit>()
                {
                    Condition = Constant(true),
                    Then      = new FailStep<Unit> { ErrorMessage = "Then Fail" },
                    Else      = new FailStep<Unit> { ErrorMessage = "Else Fail" },
                },
                new SingleError(
                    ErrorLocation.EmptyLocation,
                    ErrorCode.Test,
                    "Then Fail"
                )
            );

            yield return new ErrorCase(
                "Else is error",
                new If<Unit>()
                {
                    Condition = Constant(false),
                    Then      = new FailStep<Unit> { ErrorMessage = "Then Fail" },
                    Else      = new FailStep<Unit> { ErrorMessage = "Else Fail" },
                },
                new SingleError(
                    ErrorLocation.EmptyLocation,
                    ErrorCode.Test,
                    "Else Fail"
                )
            );
        }
    }
}
