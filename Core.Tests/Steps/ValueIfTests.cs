using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class ValueIfTests : StepTestBase<If<int>, int>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "ValueIf true",
                new If<int>()
                {
                    Condition = Constant(true), Then = Constant(1), Else = Constant(2)
                },
                1
            );

            yield return new StepCase(
                "ValueIf false",
                new If<int>()
                {
                    Condition = Constant(false), Then = Constant(1), Else = Constant(2)
                },
                2
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
                1
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
                    Condition = new FailStep<bool> { ErrorMessage = "Condition Fail" },
                    Then      = new FailStep<Unit> { ErrorMessage = "Then Fail" },
                    Else      = new FailStep<Unit> { ErrorMessage = "Else Fail" },
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
