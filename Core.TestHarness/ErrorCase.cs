using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
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
        public class ErrorCase :  ICaseThatExecutes
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
                var fileSystemMock = factory.Create<IFileSystemHelper>();

                foreach (var action in _externalProcessRunnerActions) action(externalProcessRunnerMock);

                foreach (var fileSystemAction in _fileSystemActions) fileSystemAction(fileSystemMock);

                var sfs = StepFactoryStoreToUse.Unwrap(StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep)));

                var stateMonad = new StateMonad(logger, Settings, externalProcessRunnerMock.Object, fileSystemMock.Object, sfs);

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

            /// <inheritdoc />
            public Maybe<StepFactoryStore> StepFactoryStoreToUse { get; set; }


            /// <inheritdoc />
            public ISettings Settings { get; set; } = EmptySettings.Instance;

            /// <inheritdoc />
            public void AddExternalProcessRunnerAction(Action<Mock<IExternalProcessRunner>> action) => _externalProcessRunnerActions.Add(action);

            /// <inheritdoc />
            public void AddFileSystemAction(Action<Mock<IFileSystemHelper>> action) => _fileSystemActions.Add(action);

            private readonly List<Action<Mock<IExternalProcessRunner>>> _externalProcessRunnerActions = new List<Action<Mock<IExternalProcessRunner>>>();

            private readonly List<Action<Mock<IFileSystemHelper>>> _fileSystemActions = new List<Action<Mock<IFileSystemHelper>>>();
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