namespace Reductech.Sequence.Core.Tests.Steps;

public partial class AssertTrueTests : StepTestBase<AssertTrue, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Is true true",
                "AssertTrue Boolean: true",
                Unit.Default
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Is true true",
                new AssertTrue { Boolean = Constant(true) },
                Unit.Default
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "Failed Assertion",
                new AssertTrue { Boolean = Constant(false) },
                new ErrorBuilder(ErrorCode.AssertionFailed, Constant(false).Name)
            );

            foreach (var errorCase in base.ErrorCases)
                yield return errorCase;
        }
    }
}
