﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.TestHarness
{
    public abstract partial class StepTestBase<TStep, TOutput>
    {
        protected abstract IEnumerable<ErrorCase> ErrorCases { get; }
        public IEnumerable<object?[]> ErrorCaseNames => ErrorCases.Select(x => new[] {x.Name});

        [Theory]
        [NonStaticMemberData(nameof(ErrorCaseNames), true)]
        public virtual async Task Should_return_expected_errors(string errorCaseName)
        {
            await ErrorCases.FindAndRunAsync(errorCaseName, TestOutputHelper);
        }

#pragma warning disable CA1034 // Nested types should not be visible
        public class ErrorCase :  ICaseWithState
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public ErrorCase(string name, TStep step, IError expectedError)
            {
                Name = name;
                Step = step;
                ExpectedError = expectedError;
            }

            public ErrorCase(string name, TStep step, IErrorBuilder expectedErrorBuilder)
            {
                Name = name;
                Step = step;
                ExpectedError = expectedErrorBuilder.WithLocation(step);
            }

            public string Name { get; }

            /// <inheritdoc />
            public async Task RunCaseAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
            {
                testOutputHelper.WriteLine(Step.Name);

                var logger = new TestLogger();

                var factory = new MockRepository(MockBehavior.Strict);
                var externalProcessRunnerMock = factory.Create<IExternalProcessRunner>();

                var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep));

                SetupMockExternalProcessRunner?.Invoke(externalProcessRunnerMock);
                var stateMonad = new StateMonad(logger, EmptySettings.Instance, externalProcessRunnerMock.Object, sfs);

                foreach (var (key, value) in InitialState)
                    stateMonad.SetVariable(key, value).ShouldBeSuccessful(x => x.AsString);

                var output = await Step.Run<TOutput>(stateMonad, CancellationToken.None);

                output.ShouldBeFailure(ExpectedError);

                factory.VerifyAll();
            }

            public TStep Step { get; }
            public IError ExpectedError { get; }

            public Dictionary<VariableName, object> InitialState { get; } = new Dictionary<VariableName, object>();
            public Dictionary<VariableName, object> ExpectedFinalState { get; } = new Dictionary<VariableName, object>();

            public Action<Mock<IExternalProcessRunner>>? SetupMockExternalProcessRunner { get; set; }
        }
    }
}