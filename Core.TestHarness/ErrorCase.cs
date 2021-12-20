namespace Reductech.Sequence.Core.TestHarness;

public abstract partial class StepTestBase<TStep, TOutput>
{
    [GenerateAsyncTheory("ExpectError")]
    protected virtual IEnumerable<ErrorCase> ErrorCases => CreateDefaultErrorCases();

    #pragma warning disable CA1034 // Nested types should not be visible
    public record ErrorCase : CaseThatExecutes
        #pragma warning restore CA1034 // Nested types should not be visible
    {
        public ErrorCase(string name, IStep step, IError expectedError) : base(
            name,
            ArraySegment<string>.Empty
        )
        {
            Step               = step;
            ExpectedError      = expectedError;
            IgnoreLoggedValues = true;
        }

        public ErrorCase(string name, IStep step, IErrorBuilder expectedErrorBuilder) : this(
            name,
            step,
            expectedErrorBuilder.WithLocation(step)
        ) { }

        public IStep Step { get; }
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

        foreach (var (step, expectedError, actions) in enumerable)
        {
            var errorCase = new ErrorCase(expectedError.AsString, step, expectedError)
            {
                IgnoreFinalState = true
            };

            errorCase.InitialStateActions.AddRange(actions);
            yield return errorCase;
        }
    }
}
