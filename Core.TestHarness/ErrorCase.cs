using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Reductech.EDR.Core.TestHarness
{
    public abstract partial class StepTestBase<TStep, TOutput>
    {
        protected virtual IEnumerable<ErrorCase> ErrorCases => CreateDefaultErrorCases();

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
            public ErrorCase(string name, IStep step, IError expectedError) : base(ArraySegment<object>.Empty)
            {
                Name = name;
                Step = step;
                ExpectedError = expectedError;
                IgnoreLoggedValues = true;
            }

            public ErrorCase(string name, IStep step, IErrorBuilder expectedErrorBuilder)
                : this(name, step, expectedErrorBuilder.WithLocation(step)) { }

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
                if(result.IsSuccess)
                    throw new XunitException($"Expected {ExpectedError.AsString} but was successful");

                result.Error.Should().Be(ExpectedError);
            }

            /// <inheritdoc />
            public override void CheckOutputResult(Result<TOutput, IError> result)
            {
                var result2 = result.Bind(GetValue);

                result2.Error.Should().Be(ExpectedError);
            }
        }

        private static Result<Unit, IError> GetValue(TOutput result)
        {
            if (result is IArray list)
            {
                var r = list.GetObjectsAsync(CancellationToken.None).Result;

                if (r.IsFailure) return r.ConvertFailure<Unit>();
            }

            return Unit.Default;
        }

        /// <summary>
        /// Creates the default error cases.
        /// These tests that for a particular property, if that property returns an error then the step itself will return an error.
        /// </summary>
        protected static IEnumerable<ErrorCase> CreateDefaultErrorCases(object? defaultVariableValue = null)
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
}