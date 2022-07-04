namespace Reductech.Sequence.Core.Tests.Steps;

public partial class TryTests : StepTestBase<Try<SCLInt>, SCLInt>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield break;
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "Statement and Alternative both fail",
                new Try<SCLInt>()
                {
                    Statement = new FailStep<SCLInt>() { ErrorMessage = "Statement Failed" },
                    Recover = new LambdaFunction<StringStream, SCLInt>(
                        null,
                        new FailStep<SCLInt>() { ErrorMessage = "Alternative Failed" }
                    )
                },
                ErrorCode.Test.ToErrorBuilder("Alternative Failed")
                    .WithLocationSingle(ErrorLocation.EmptyLocation)
            );
        }
    }
}
