namespace Reductech.Sequence.Core.TestHarness;

public abstract partial class StepTestBase<TStep, TOutput>
{
    /// <summary>
    /// Error cases for this step
    /// </summary>
    [GenerateAsyncTheory("ExpectError")]
    protected virtual IEnumerable<ErrorCase> ErrorCases => CreateDefaultErrorCases();

    /// <summary>
    /// A case that tests a particular error condition
    /// </summary>
    public record ErrorCase : CaseThatExecutes
    {
        /// <summary>
        /// Create a new ErrorCase
        /// </summary>
        public ErrorCase(string name, IStep step, IError expectedError) : base(
            name,
            ArraySegment<string>.Empty
        )
        {
            Step               = step;
            ExpectedError      = expectedError;
            IgnoreLoggedValues = true;
        }

        /// <summary>
        /// Create a new ErrorCase
        /// </summary>
        public ErrorCase(string name, IStep step, IErrorBuilder expectedErrorBuilder) : this(
            name,
            step,
            expectedErrorBuilder.WithLocation(step)
        ) { }

        /// <summary>
        /// The step to test
        /// </summary>
        public IStep Step { get; }

        /// <summary>
        /// The expected error
        /// </summary>
        public IError ExpectedError { get; }

        /// <inheritdoc />
        public override async Task<IStep> GetStepAsync(
            IExternalContext externalContext,
            ITestOutputHelper testOutputHelper)
        {
            await Task.CompletedTask;
            return Step;
        }

        /// <inheritdoc />
        public override void CheckUnitResult(Result<Unit, IError> result)
        {
            if (result.IsSuccess)
                throw new XunitException($"Expected {ExpectedError.AsString} but was successful");

            result.Error.Should().Be(ExpectedError);
        }

        /// <inheritdoc />
        public override void CheckOutputResult(Result<TOutput, IError> result)
        {
            var result2 = result.Bind(GetValue);
            result2.ShouldBeFailure();

            result2.Error.Should().Be(ExpectedError);
        }

        /// <inheritdoc />
        public override bool ShouldVerify => false;
    }

    private static Result<Unit, IError> GetValue(TOutput result)
    {
        if (result is IArray list)
        {
            var r = list.GetObjectsAsync(CancellationToken.None).Result;

            if (r.IsFailure)
                return r.ConvertFailure<Unit>();
        }

        return Unit.Default;
    }

    /// <summary>
    /// Creates the default error cases.
    /// These tests that for a particular property, if that property returns an error then the step itself will return an error.
    /// </summary>
    protected static IEnumerable<ErrorCase> CreateDefaultErrorCases(
        ISCLObject? defaultVariableValue = null)
    {
        var enumerable = CreateStepsWithFailStepsAsValues(defaultVariableValue);

        foreach (var (step, expectedError, injectedVariables) in enumerable)
        {
            var errorCase = new ErrorCase(expectedError.AsString, step, expectedError)
            {
                IgnoreFinalState = true
            };

            foreach (var (variableName, sclObject) in injectedVariables)
            {
                errorCase.InjectedVariables.Add(
                    variableName,
                    new InjectedVariable(sclObject, null)
                );
            }

            yield return errorCase;
        }
    }
}
