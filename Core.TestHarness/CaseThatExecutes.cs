using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using MELT;
using Microsoft.Extensions.Logging;
using Moq;
using Namotion.Reflection;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
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
            protected CaseThatExecutes(IReadOnlyCollection<object> expectedLoggedValues) =>
                ExpectedLoggedValues = expectedLoggedValues;

            /// <inheritdoc />
            public async Task RunCaseAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
            {
                var loggerFactory = TestLoggerFactory.Create();
                loggerFactory.AddXunit(testOutputHelper);

                var step = await GetStepAsync(testOutputHelper, extraArgument);

                var mockRepository = new MockRepository(MockBehavior.Strict);

                using var stateMonad = GetStateMonad(mockRepository, loggerFactory.CreateLogger("Test"));

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
                {
                    var stepType = step.GetType().GetDisplayName();
                    throw new XunitException(
                        $"{stepType} does not have output type {nameof(Unit)} or {typeof(TOutput).Name}");
                }

                CheckLoggedValues(loggerFactory);

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

            public virtual void CheckLoggedValues(ITestLoggerFactory loggerFactory)
            {
                if (!IgnoreLoggedValues)
                {
                    StaticHelpers.CheckLoggedValues(loggerFactory, LogLevel.Information, ExpectedLoggedValues);
                }
            }

            public virtual StateMonad GetStateMonad(MockRepository mockRepository, ILogger logger)
            {
                var externalProcessRunner = GetExternalProcessRunner(mockRepository);
                var fileSystemHelper = GetFileSystemHelper(mockRepository);
                var sfs = StepFactoryStoreToUse.Unwrap(
                    StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep)));

                var stateMonad = new StateMonad(logger, Settings, externalProcessRunner, fileSystemHelper, sfs);

                foreach (var action in InitialStateActions)
                    action(stateMonad);

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
            public void AddExternalProcessRunnerAction(Action<Mock<IExternalProcessRunner>> action) =>
                _externalProcessRunnerActions.Add(action);

            /// <inheritdoc />
            public void AddFileSystemAction(Action<Mock<IFileSystemHelper>> action) => _fileSystemActions.Add(action);

            private readonly List<Action<Mock<IExternalProcessRunner>>> _externalProcessRunnerActions = new();

            private readonly List<Action<Mock<IFileSystemHelper>>> _fileSystemActions = new();


            /// <inheritdoc />
            public bool IgnoreFinalState { get; set; }

            public bool IgnoreLoggedValues { get; set; }

            /// <inheritdoc />
            public Maybe<StepFactoryStore> StepFactoryStoreToUse { get; set; }

            public List<Action<IStateMonad>> InitialStateActions { get; } = new();

            /// <inheritdoc />
            public ISettings Settings { get; set; } = EmptySettings.Instance;

            public Dictionary<VariableName, object> ExpectedFinalState { get; } = new();

            public IReadOnlyCollection<object> ExpectedLoggedValues { get; }
        }
    }
}