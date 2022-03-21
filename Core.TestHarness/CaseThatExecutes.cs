using Reductech.Sequence.Core.TestHarness.Rest;

namespace Reductech.Sequence.Core.TestHarness;

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

            var mockRepository = new MockRepository(MockBehavior.Strict);

            //var restClient = RESTClientSetupHelper.GetRESTClient(mockRepository, FinalChecks);
            var restClientFactory = RESTClientSetupHelper.GetRESTClientFactory(mockRepository);

            var externalContext =
                ExternalContextSetupHelper.GetExternalContext(mockRepository, restClientFactory);

            var step = await GetStepAsync(externalContext, testOutputHelper);

            if (!ShouldExecute)
                return;

            await using var stateMonad = await GetStateMonad(
                externalContext,
                loggerFactory.CreateLogger("Test")
            );

            if (ShouldVerify)
            {
                var verifyResult = step.Verify(stateMonad.StepFactoryStore);
                verifyResult.ShouldBeSuccessful();
            }

            if (step is IStep<TOutput> outputStep)
            {
                var result = await outputStep.Run<TOutput>(stateMonad, CancellationToken.None);
                CheckOutputResult(result);
            }
            else if (step is IStep<ISCLObject> objectStep)
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
                var result = await step.Run<ISCLObject>(stateMonad, CancellationToken.None);
                CheckObjectResult(step, result);
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

            foreach (var finalCheck in FinalChecks)
            {
                finalCheck();
            }
        }

        /// <summary>
        /// Should the SCL be verified before running
        /// </summary>
        public virtual bool ShouldVerify => true;

        /// <inheritdoc />
        public override string ToString() => Name;

        public virtual LogLevel OutputLogLevel { get; } = LogLevel.Debug;

        public abstract Task<IStep> GetStepAsync(
            IExternalContext externalContext,
            ITestOutputHelper testOutputHelper);

        public abstract void CheckUnitResult(Result<Unit, IError> result);
        public abstract void CheckOutputResult(Result<TOutput, IError> result);

        public virtual void CheckObjectResult(IStep step, Result<ISCLObject, IError> result)
        {
            var stepType = step.GetType().GetDisplayName();

            throw new XunitException(
                $"{stepType} does not have output type {nameof(Unit)} or {typeof(TOutput).Name}"
            );
        }

        public virtual void CheckLoggedValues(ITestLoggerFactory loggerFactory)
        {
            if (!IgnoreLoggedValues)
            {
                LogChecker.CheckLoggedValues(
                    loggerFactory,
                    CheckLogLevel,
                    ExpectedLoggedValues
                );
            }
        }

        public virtual async Task<StateMonad> GetStateMonad(
            IExternalContext externalContext,
            ILogger logger)
        {
            var tStepAssembly = Assembly.GetAssembly(typeof(TStep))!;

            var sfs = StepFactoryStoreToUse.GetValueOrDefault(
                StepFactoryStore.TryCreateFromAssemblies(
                        externalContext,
                        tStepAssembly
                    )
                    .GetOrThrow()
            );

            var stateMonad = new StateMonad(
                logger,
                sfs,
                externalContext,
                new Dictionary<string, object>()
            );

            var injectResult = await stateMonad.SetInitialVariablesAsync(InjectedVariables);
            injectResult.ShouldBeSuccessful();

            return stateMonad;
        }

        public ExternalContextSetupHelper ExternalContextSetupHelper { get; } = new();

        /// <inheritdoc />
        public RESTClientSetupHelper RESTClientSetupHelper { get; } = new();

        /// <inheritdoc />
        public bool IgnoreFinalState { get; set; }

        public bool IgnoreLoggedValues { get; set; }

        /// <summary>
        /// Whether the SCL should be executed
        /// </summary>
        public virtual bool ShouldExecute => true;

        /// <inheritdoc />
        public Maybe<StepFactoryStore> StepFactoryStoreToUse { get; set; }

        public LogLevel CheckLogLevel { get; set; } = LogLevel.Information;

        public Dictionary<VariableName, ISCLObject> ExpectedFinalState { get; } = new();
        public Dictionary<VariableName, ISCLObject> InjectedVariables { get; } = new();
        public string Name { get; set; } = Name;

        public IReadOnlyCollection<string> ExpectedLoggedValues { get; set; } =
            ExpectedLoggedValues;

        public List<Action<IExternalContext>> FinalContextChecks { get; } = new();

        public List<Action> FinalChecks { get; } = new();
    }
}
