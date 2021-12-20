namespace Reductech.EDR.Core.Tests.Steps;

public partial class AssertErrorTests : StepTestBase<AssertError, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Log Divide by zero",
                "AssertError Step: (Log Value: (1 / 0))",
                Unit.Default,
                "Log Started with Parameters: [Value, (1 / 0)]",
                "Divide Started with Parameters: [Terms, [1, 0]]",
                "ArrayNew Started with Parameters: [Elements, [1, 0]]",
                "ArrayNew Completed Successfully with Result: [1, 0]",
                "Divide Failed with message: Attempt to Divide by Zero.",
                "Log Failed with message: Attempt to Divide by Zero."
            ).WithCheckLogLevel(LogLevel.Trace);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Log divide by zero",
                new AssertError { Step = new Log { Value = new Divide() { Terms = Array(1, 0) } } },
                Unit.Default,
                "Log Started with Parameters: [Value, (1 / 0)]",
                "Divide Started with Parameters: [Terms, [1, 0]]",
                "ArrayNew Started with Parameters: [Elements, [1, 0]]",
                "ArrayNew Completed Successfully with Result: [1, 0]",
                "Divide Failed with message: Attempt to Divide by Zero.",
                "Log Failed with message: Attempt to Divide by Zero."
            ).WithCheckLogLevel(LogLevel.Trace);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "Successful Step",
                new AssertError { Step = new Log { Value = Constant("Hello World") } },
                new ErrorBuilder(ErrorCode.AssertionFailed, Constant("Log").Name)
            );
        }
    }
}
