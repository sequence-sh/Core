using System.Net;
using Reductech.Sequence.Core.ExternalProcesses;
using Reductech.Sequence.Core.TestHarness.Rest;

namespace Reductech.Sequence.Core.TestHarness;

/// <summary>
/// Extension methods for Test Harness
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Checks that this step produces a particular final state
    /// </summary>
    public static T WithExpectedFinalState<T>(this T cws, string variableName, object? value)
        where T : ICaseThatExecutes
    {
        var sclObject = ISCLObject.CreateFromCSharpObject(value);

        cws.ExpectedFinalState.Add(new VariableName(variableName), sclObject);
        return cws;
    }

    /// <summary>
    /// Check that this step injects a particular variable
    /// </summary>
    public static T WithInjectedVariable<T>(this T cws, string variableName, object? value)
        where T : ICaseWithSetup
    {
        var sclObject = ISCLObject.CreateFromCSharpObject(value);

        cws.InjectedVariables.Add(
            new VariableName(variableName),
            new InjectedVariable(sclObject, null)
        );

        return cws;
    }

    /// <summary>
    /// Run this step with a particular StepFactoryStore
    /// </summary>
    public static T WithStepFactoryStore<T>(this T cws, StepFactoryStore stepFactoryStore)
        where T : ICaseThatExecutes
    {
        cws.StepFactoryStoreToUse = stepFactoryStore;
        return cws;
    }

    /// <summary>
    /// Checks the final context after the step has finished
    /// </summary>
    public static T WithFinalContextCheck<T>(this T cws, Action<IExternalContext> contextCheck)
        where T : ICaseThatExecutes
    {
        cws.FinalContextChecks.Add(contextCheck);
        return cws;
    }

    /// <summary>
    /// Checks a rest request
    /// </summary>
    public static bool CheckRequest(
        this RestRequest request,
        (string resource, Method method, object? body) expected)
    {
        if (request.Method != expected.method)
            return false;

        var realURl = request.GetFullResource();

        if (realURl != expected.resource)
            return false;

        var realBody = request.GetRequestBody();

        if (!Equals(realBody, expected.body))
            return false;

        return true;
    }

    /// <summary>
    /// Checks the resource of a REST request
    /// </summary>
    public static string GetFullResource(this RestRequest request)
    {
        var resource = request.Resource;

        foreach (var parameter in request.Parameters.Where(p => p.Type == ParameterType.UrlSegment))
        {
            var oldValue = "{" + parameter.Name + "}";

            resource = resource.Replace(oldValue, parameter.Value?.ToString());
        }

        var result = resource.TrimEnd('/');
        return result;
    }

    /// <summary>
    /// Gets the body of a REST request
    /// </summary>
    public static object? GetRequestBody(this RestRequest request)
    {
        foreach (var parameter in
                 request.Parameters.Where(x => x.Type == ParameterType.RequestBody))
        {
            return parameter.Value;
        }

        return null;
    }

    /// <summary>
    /// Add an additional External Process Action
    /// </summary>
    public static T WithExternalProcessAction<T>(
        this T cws,
        Action<Mock<IExternalProcessRunner>> action) where T : ICaseWithSetup => WithAction(
        cws,
        new Action<Mock<IExternalProcessRunner>, MockRepository>((a, _) => action(a))
    );

    /// <summary>
    /// Add an additional Console Action
    /// </summary>
    public static T WithConsoleAction<T>(this T cws, Action<Mock<IConsole>> action)
        where T : ICaseWithSetup => WithAction(
        cws,
        new Action<Mock<IConsole>, MockRepository>((a, _) => action(a))
    );

    /// <summary>
    /// Add an additional Console Action
    /// </summary>
    public static T WithConsoleAction<T>(this T cws, Action<Mock<IConsole>, MockRepository> action)
        where T : ICaseWithSetup => WithAction(cws, action);

    private static T WithAction<T, TObject>(
        this T cws,
        Action<Mock<TObject>, MockRepository> action)
        where T : ICaseWithSetup
        where TObject : class
    {
        cws.ExternalContextSetupHelper.AddSetupAction(action);
        return cws;
    }

    /// <summary>
    /// Setup an HTTP request
    /// </summary>
    public static T SetupHTTPSuccess<T>(
        this T stepCase,
        string baseUri,
        (string resource,
            Method method,
            object? body) request,
        bool responseSuccess,
        HttpStatusCode responseStatusCode,
        string responseContent = "")
        where T : ICaseWithSetup
    {
        var response = new RestResponse()
        {
            Content             = responseContent,
            ContentLength       = responseContent.Length,
            ResponseStatus      = ResponseStatus.Completed,
            StatusCode          = responseStatusCode,
            IsSuccessStatusCode = true,
        };

        stepCase.RESTClientSetupHelper.AddHttpTestAction(
            new RESTSetup(
                baseUri,
                x => x.CheckRequest(request),
                response
            )
        );

        return stepCase;
    }

    /// <summary>
    /// Setup an HTTP request
    /// </summary>
    public static T SetupHTTPError<T>(
        this T stepCase,
        string baseUri,
        (string resource,
            Method method,
            object? body) request,
        HttpStatusCode responseStatusCode,
        string statusDescription,
        string error)
        where T : ICaseWithSetup
    {
        var response = new RestResponse()
        {
            ResponseStatus    = ResponseStatus.Error,
            StatusCode        = responseStatusCode,
            StatusDescription = statusDescription,
            ErrorMessage      = error,
        };

        stepCase.RESTClientSetupHelper.AddHttpTestAction(
            new RESTSetup(
                baseUri,
                x => x.CheckRequest(request),
                response
            )
        );

        return stepCase;
    }

    /// <summary>
    /// Add an additional context to this Step
    /// </summary>
    public static T WithContext<T>(this T cws, string name, object context)
        where T : ICaseWithSetup
    {
        cws.ExternalContextSetupHelper.AddContextObject(name, context);
        return cws;
    }

    /// <summary>
    /// Add an additional context mock to this Step
    /// </summary>
    public static T WithContextMock<T>(
        this T cws,
        string name,
        Func<MockRepository, Mock> function)
        where T : ICaseWithSetup
    {
        cws.ExternalContextSetupHelper.AddContextMock(name, function);
        return cws;
    }

    /// <summary>
    /// Set the minimum log level to check
    /// </summary>
    public static T WithCheckLogLevel<T>(this T cte, LogLevel logLevel)
        where T : ICaseThatExecutes
    {
        cte.CheckLogLevel = logLevel;
        return cte;
    }

    /// <summary>
    /// Asserts that this task should result in success.
    /// </summary>
    public static async Task ShouldSucceed(this Task<Result> resultTask)
    {
        var r = await resultTask;
        r.ShouldBeSuccessful();
    }

    /// <summary>
    /// Asserts that this task should result in success.
    /// </summary>
    public static async Task ShouldSucceed<T>(this Task<Result<T>> resultTask)
    {
        var r = await resultTask;
        r.ShouldBeSuccessful();
    }

    /// <summary>
    /// Asserts that this result should equal the expected result.
    /// </summary>
    public static void ShouldBe<T>(this Result<T> result, Result<T> expectedResult)
    {
        if (expectedResult.IsSuccess)
        {
            result.ShouldBeSuccessful();
            result.Value.Should().Be(expectedResult.Value);
        }
        else
            result.ShouldBeFailure(expectedResult.Error);
    }

    /// <summary>
    /// Asserts that this result was successful.
    /// </summary>
    public static void ShouldBeSuccessful(this Result result)
    {
        var (_, isFailure, error) = result;
        Assert.False(isFailure, error);
    }

    /// <summary>
    /// Asserts that this result was successful.
    /// </summary>
    public static void ShouldBeSuccessful<T>(this Result<T, IError> result)
    {
        var (_, isFailure, _, error) = result;

        if (isFailure)
        {
            throw new XunitException(error.AsStringWithLocation);
        }
    }

    /// <summary>
    /// Asserts that this result was successful.
    /// </summary>
    public static void ShouldBeSuccessful<T>(this Result<T, IErrorBuilder> result)
    {
        var (_, isFailure, _, error) = result;

        if (isFailure)
        {
            throw new XunitException(error.AsString);
        }
    }

    /// <summary>
    /// Asserts that this result was successful.
    /// </summary>
    public static void ShouldBeSuccessful<T>(this Result<T> result)
    {
        var (_, isFailure, _, error) = result;

        if (isFailure)
            throw new XunitException(error);
    }

    /// <summary>
    /// Asserts that this result was a failure.
    /// </summary>
    public static void ShouldBeFailure(this Result result, string? expectedError = null)
    {
        result.IsFailure.Should().BeTrue("The result was expected to fail");

        if (expectedError != null)
            result.Error.Should().Be(expectedError);
    }

    /// <summary>
    /// Asserts that this result was a failure.
    /// </summary>
    public static void ShouldBeFailure<T>(this Result<T> result, string? expectedError = null)
    {
        result.IsFailure.Should().BeTrue("The result was expected to fail");

        if (expectedError != null)
            result.Error.Should().Be(expectedError);
    }

    /// <summary>
    /// Asserts that this result was a failure.
    /// </summary>
    public static void ShouldBeFailure<T, TE>(this Result<T, TE> result) =>
        result.IsFailure.Should().BeTrue();

    /// <summary>
    /// Asserts that this result was a failure.
    /// </summary>
    public static void ShouldBeFailure<T, TE>(this Result<T, TE> result, TE expectedError)
    {
        result.IsFailure.Should().BeTrue("The result was expected to fail");

        result.Error.Should().Be(expectedError);
    }

    /// <summary>
    /// Asserts this maybe should have a value
    /// </summary>
    public static void ShouldHaveValue<T>(this Maybe<T> maybe)
    {
        maybe.HasValue.Should().BeTrue("Maybe should have value");
    }

    /// <summary>
    /// Asserts this maybe should have no value
    /// </summary>
    public static void ShouldHaveNoValue<T>(this Maybe<T> maybe)
    {
        maybe.HasValue.Should().BeFalse("Maybe should have no value");
    }
}
