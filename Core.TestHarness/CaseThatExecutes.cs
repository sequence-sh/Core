using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Reductech.EDR.Core.TestHarness
{
    public abstract partial class StepTestBase<TStep, TOutput>
    {
#pragma warning disable CA1034 // Nested types should not be visible
        public abstract class CaseThatExecutes : ICaseThatExecutes
#pragma warning restore CA1034 // Nested types should not be visible
        {
            protected CaseThatExecutes(IReadOnlyCollection<object> expectedLoggedValues) => ExpectedLoggedValues = expectedLoggedValues;

            /// <inheritdoc />
            public async Task RunCaseAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
            {
                var logger = new TestLogger();

                var step = await GetStepAsync(testOutputHelper, extraArgument);

                testOutputHelper.WriteLine(step.Name);

                var mockRepository = new MockRepository(MockBehavior.Strict);

                using var stateMonad = GetStateMonad(mockRepository, logger);

                if (step is IStep<TOutput> outputStep)
                {
                    var result = await outputStep.Run<TOutput>(stateMonad, CancellationToken.None);

                    CheckOutputResult(result);
                }
                else if (step is IStep<Unit> unitStep)
                {
                    var result = await unitStep.Run<Unit>(stateMonad, CancellationToken.None);

                    CheckUnitResult(result);
                }
                else
                    throw new XunitException($"Step is does not have output type {nameof(Unit)} or {nameof(TOutput)}");

                if(!IgnoreLoggedValues)
                    logger.LoggedValues.Select(x=>CompressNewlines(x.ToString()!)) .Should().BeEquivalentTo(ExpectedLoggedValues);

                if (!IgnoreFinalState)
                    stateMonad.GetState().Should().BeEquivalentTo(ExpectedFinalState);

                mockRepository.VerifyAll();
            }

            public abstract string Name { get; }

            /// <inheritdoc />
            public override string ToString() => Name;

            public abstract Task<IStep> GetStepAsync(ITestOutputHelper testOutputHelper, string? extraArgument);

            public abstract void CheckUnitResult(Result<Unit, IError> result);
            public abstract void CheckOutputResult(Result<TOutput, IError> result);

            public virtual StateMonad GetStateMonad(MockRepository mockRepository, ILogger logger)
            {

                var externalProcessRunner = GetExternalProcessRunner(mockRepository);
                var fileSystemHelper = GetFileSystemHelper(mockRepository);

                var sfs = StepFactoryStoreToUse.Unwrap(StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep)));

                var stateMonad = new StateMonad(logger, Settings, externalProcessRunner, fileSystemHelper, sfs);


                return stateMonad;
            }



            public virtual IExternalProcessRunner GetExternalProcessRunner(MockRepository mockRepository)
            {
                var mock = mockRepository.Create<IExternalProcessRunner>();

                foreach (var action in _externalProcessRunnerActions) action(mock);

                return mock.Object;
            }

            public virtual IFileSystemHelper GetFileSystemHelper(MockRepository mockRepository)
            {
                var mock = mockRepository.Create<IFileSystemHelper>();

                foreach (var action in _fileSystemActions) action(mock);

                return mock.Object;
            }

            /// <inheritdoc />
            public void AddExternalProcessRunnerAction(Action<Mock<IExternalProcessRunner>> action) => _externalProcessRunnerActions.Add(action);

            /// <inheritdoc />
            public void AddFileSystemAction(Action<Mock<IFileSystemHelper>> action) => _fileSystemActions.Add(action);

            private readonly List<Action<Mock<IExternalProcessRunner>>> _externalProcessRunnerActions = new List<Action<Mock<IExternalProcessRunner>>>();

            private readonly List<Action<Mock<IFileSystemHelper>>> _fileSystemActions = new List<Action<Mock<IFileSystemHelper>>>();


            /// <inheritdoc />
            public bool IgnoreFinalState { get; set; }

            public bool IgnoreLoggedValues { get; set; }

            /// <inheritdoc />
            public Maybe<StepFactoryStore> StepFactoryStoreToUse { get; set; }


            /// <inheritdoc />
            public ISettings Settings { get; set; } = EmptySettings.Instance;

            public Dictionary<VariableName, object> ExpectedFinalState { get; } = new Dictionary<VariableName, object>();

            public IReadOnlyCollection<object> ExpectedLoggedValues { get; }

        }

    }
}
