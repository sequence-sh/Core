namespace Reductech.Sequence.Core.Tests.Steps;

public partial class ValueIfTests : StepTestBase<If<SCLInt>, SCLInt>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "ValueIf true",
                new If<SCLInt>()
                {
                    Condition = Constant(true), Then = Constant(1), Else = Constant(2)
                },
                1.ConvertToSCLObject()
            );

            yield return new StepCase(
                "ValueIf false",
                new If<SCLInt>()
                {
                    Condition = Constant(false), Then = Constant(1), Else = Constant(2)
                },
                2.ConvertToSCLObject()
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "ValueIf true",
                "ValueIf Condition: true Then: 1 Else: 2",
                1.ConvertToSCLObject()
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
