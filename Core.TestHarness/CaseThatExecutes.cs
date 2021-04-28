﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Divergic.Logging.Xunit;
using FluentAssertions;
using MELT;
using Microsoft.Extensions.Logging;
using Moq;
using Namotion.Reflection;
using Reductech.EDR.Core.Abstractions;
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
    public abstract record CaseThatExecutes(
            string Name,
            IReadOnlyCollection<string> ExpectedLoggedValues) : ICaseThatExecutes
        #pragma warning restore CA1034 // Nested types should not be visible
    {
        /// <inheritdoc />
        public async Task RunAsync(ITestOutputHelper testOutputHelper)
        {
            var loggerFactory =
                TestLoggerFactory.Create();

            loggerFactory.AddXunit(
                testOutputHelper,
                new LoggingConfig() { LogLevel = OutputLogLevel }
            );

            var step = await GetStepAsync(testOutputHelper);

            var mockRepository = new MockRepository(MockBehavior.Strict);

            await using var stateMonad = await GetStateMonad(
                mockRepository,
                loggerFactory.CreateLogger("Test")
            );

            if (step is IStep<TOutput> outputStep)
            {
                var result = await outputStep.Run<TOutput>(stateMonad, CancellationToken.None);
                CheckOutputResult(result);
            }
            else if (step is IStep<object> objectStep)
            {
                var result = await objectStep.Run<TOutput>(stateMonad, CancellationToken.None);
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
                    $"{stepType} does not have output type {nameof(Unit)} or {typeof(TOutput).Name}"
                );
            }

            CheckLoggedValues(loggerFactory);

            if (!IgnoreFinalState)
                stateMonad.GetState()
                    .Should()
                    .BeEquivalentTo(
                        ExpectedFinalState,
                        "Final State should match Expected Final State"
                    );

            mockRepository.VerifyAll();

            foreach (var finalContextCheck in FinalContextChecks)
            {
                finalContextCheck(stateMonad.ExternalContext);
            }
        }

        /// <inheritdoc />
        public override string ToString() => Name;

        public virtual LogLevel OutputLogLevel { get; } = LogLevel.Debug;

        public abstract Task<IStep> GetStepAsync(ITestOutputHelper testOutputHelper);

        public abstract void CheckUnitResult(Result<Unit, IError> result);
        public abstract void CheckOutputResult(Result<TOutput, IError> result);

        public virtual void
            CheckLoggedValues(ITestLoggerFactory loggerFactory) //Only check information or above???
        {
            if (!IgnoreLoggedValues)
            {
                LogChecker.CheckLoggedValues(
                    loggerFactory,
                    LogLevel.Information,
                    ExpectedLoggedValues
                );
            }
        }

        public virtual async Task<StateMonad> GetStateMonad(
            MockRepository mockRepository,
            ILogger logger)
        {
            var externalContext = ExternalContextSetupHelper.GetExternalContext(mockRepository);

            var tStepAssembly = Assembly.GetAssembly(typeof(TStep))!;

            var sfs = StepFactoryStoreToUse.Unwrap(
                StepFactoryStore.Create(Settings, tStepAssembly)
            );

            var stateMonad = new StateMonad(
                logger,
                Settings,
                sfs,
                externalContext,
                new Dictionary<string, object>()
            );

            foreach (var action in InitialStateActions)
                await action(stateMonad);

            return stateMonad;
        }

        public ExternalContextSetupHelper ExternalContextSetupHelper { get; } = new();

        /// <inheritdoc />
        public bool IgnoreFinalState { get; set; }

        public bool IgnoreLoggedValues { get; set; }

        /// <inheritdoc />
        public Maybe<StepFactoryStore> StepFactoryStoreToUse { get; set; }

        public List<Func<IStateMonad, Task>> InitialStateActions { get; } = new();

        /// <inheritdoc />
        public SCLSettings Settings { get; set; } = SCLSettings.EmptySettings;

        public Dictionary<VariableName, object> ExpectedFinalState { get; } = new();
        public string Name { get; set; } = Name;

        public IReadOnlyCollection<string> ExpectedLoggedValues { get; set; } =
            ExpectedLoggedValues;

        public List<Action<IExternalContext>> FinalContextChecks { get; } = new();
    }
}

}
