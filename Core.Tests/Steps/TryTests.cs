using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class TryTests : StepTestBase<Try<int>, int>
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
                new Try<int>()
                {
                    Statement   = new FailStep<int>() { ErrorMessage = "Statement Failed" },
                    Alternative = new FailStep<int>() { ErrorMessage = "Alternative Failed" },
                },
                ErrorCode.Test.ToErrorBuilder("Alternative Failed")
                    .WithLocationSingle(ErrorLocation.EmptyLocation)
            );
        }
    }
}
