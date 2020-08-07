using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Output;
using Xunit;

namespace Reductech.EDR.Processes.Tests.Extensions
{
    public static class TestExtensions
    {
        public static async Task ShouldSucceed(this Task<Result> resultTask)
        {
            var r = await resultTask;
            r.ShouldBeSuccessful();
        }

        public static async Task ShouldSucceed<T>(this Task<Result<T>> resultTask)
        {
            var r = await resultTask;
            r.ShouldBeSuccessful();
        }


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

        public static void ShouldBeSuccessful(this Result result)
        {
            var (_, isFailure, error) = result;
            Assert.False(isFailure, error);
        }


        public static void ShouldBeSuccessful<T>(this Result<T> result)
        {
            var (_, isFailure, _, error) = result;
            Assert.False(isFailure, error);
        }


        public static void ShouldBeFailure(this Result result, string? expectedError = null)
        {
            var (_, isFailure, realError) = result;
            Assert.True(isFailure);

            if (expectedError != null)
                realError.Should().Be(expectedError);
        }


        public static void ShouldBeFailure<T>(this Result<T> result, string? expectedError = null)
        {
            var (_, isFailure, _, realError) = result;
            Assert.True(isFailure);

            if (expectedError != null)
                realError.Should().Be(expectedError);
        }

        public static Result<(T result, List<string> messages)> GetResultFromImmutableProcess<T>(this IImmutableProcess<T> process, bool failOnWarning)
        {
            var enumerateResult = process.Execute().ToListAsync().Result;

            var errors = enumerateResult.Where(x => x.OutputType == OutputType.Error).Select(x => x.Text).ToList();

            if (errors.Any())
                return Result.Failure<(T, List<string>)>(string.Join("\result\n", errors));

            if (failOnWarning)
            {
                var warnings = enumerateResult.Where(x => x.OutputType == OutputType.Warning).Select(x => x.Text).ToList();

                if (warnings.Any())
                    return Result.Failure<(T, List<string>)>(string.Join("\result\n", errors));
            }

            var success = enumerateResult.SingleOrDefault(x => x.OutputType == OutputType.Success);

            if (success == null)
                return Result.Failure<(T, List<string>)>("Process had no success output");


            var messages = enumerateResult
                .Where(x => x.OutputType == OutputType.Message)
                .Select(x => x.Text).ToList();

            return (success.Value!, messages);


        }


    }
}
