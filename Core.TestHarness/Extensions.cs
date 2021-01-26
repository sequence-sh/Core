using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Moq;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Xunit;
using Xunit.Sdk;

namespace Reductech.EDR.Core.TestHarness
{

public static class Extensions
{
    public static T WithExpectedFinalState<T>(this T cws, string variableName, object value)
        where T : ICaseThatExecutes
    {
        if (value is string s)
            value = new StringStream(s);

        cws.ExpectedFinalState.Add(new VariableName(variableName), value);
        return cws;
    }

    public static T WithStepFactoryStore<T>(this T cws, StepFactoryStore stepFactoryStore)
        where T : ICaseThatExecutes
    {
        cws.StepFactoryStoreToUse = stepFactoryStore;
        return cws;
    }

    public static T WithSettings<T>(this T cws, SCLSettings settings) where T : ICaseThatExecutes
    {
        cws.Settings = settings;
        return cws;
    }

    public static T WithExternalProcessAction<T>(
        this T cws,
        Action<Mock<IExternalProcessRunner>> action) where T : ICaseThatExecutes
    {
        cws.AddExternalProcessRunnerAction(action);
        return cws;
    }

    public static T WithFileSystemAction<T>(this T cws, Action<Mock<IFileSystemHelper>> action)
        where T : ICaseThatExecutes
    {
        cws.AddFileSystemAction(action);
        return cws;
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
    public static void ShouldBeSuccessful<T, TE>(
        this Result<T, TE> result,
        Func<TE, string> convert)
    {
        var (_, isFailure, _, error) = result;

        if (isFailure)
            throw new XunitException(convert(error));
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
}

}
