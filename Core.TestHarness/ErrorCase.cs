using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.TestHarness
{
    public abstract partial class StepTestBase<TStep, TOutput>
    {
        protected virtual IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                yield return CreateDefaultErrorCase();
            }
        }

        public IEnumerable<object?[]> ErrorCaseNames => ErrorCases.Select(x => new[] {x.Name});

        [Theory]
        [NonStaticMemberData(nameof(ErrorCaseNames), true)]
        public virtual async Task Should_return_expected_errors(string errorCaseName)
        {
            await ErrorCases.FindAndRunAsync(errorCaseName, TestOutputHelper);
        }

#pragma warning disable CA1034 // Nested types should not be visible
        public class ErrorCase :  CaseThatExecutes
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public ErrorCase(string name, IStep step, IError expectedError, params string[] expectedLoggedValues) : base(expectedLoggedValues)
            {
                Name = name;
                Step = step;
                ExpectedError = expectedError;
            }

            public ErrorCase(string name, IStep step, IErrorBuilder expectedErrorBuilder, params string[] expectedLoggedValues) : this(name, step, expectedErrorBuilder.WithLocation(step), expectedLoggedValues) { }

            public override string Name { get; }


            public IStep Step { get; }
            public IError ExpectedError { get; }

            /// <inheritdoc />
            public override async Task<IStep> GetStepAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
            {
                await Task.CompletedTask;
                return Step;
            }

            /// <inheritdoc />
            public override void CheckUnitResult(Result<Unit, IError> result)
            {
                result.ShouldBeFailure(ExpectedError);
            }

            /// <inheritdoc />
            public override void CheckOutputResult(Result<TOutput, IError> result)
            {
                result.ShouldBeFailure(ExpectedError);
            }
        }

        /// <summary>
        /// Creates the default error case. This tests that if every property returns an error, that error will be propagated.
        /// If the step tries to get a variable before trying to get a property, set firstErrorIsStep to true.
        /// </summary>
        protected static ErrorCase CreateDefaultErrorCase(bool firstErrorIsStep = true)
        {
            var step = CreateStepWithFailStepsAsValues();
            var error = firstErrorIsStep ? FailStepError : new SingleError("Variable '<Foo>' does not exist.", ErrorCode.MissingVariable, new StepErrorLocation(step));
            var errorCase = new ErrorCase("Default", step, error);

            return errorCase;
        }
    }
}