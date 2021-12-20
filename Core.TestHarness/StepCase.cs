using Reductech.Sequence.Core.Internal.Parser;

namespace Reductech.Sequence.Core.TestHarness;

public abstract partial class StepTestBase<TStep, TOutput>
{
    protected abstract IEnumerable<StepCase> StepCases { get; }

    [AutoTheory.GenerateAsyncTheory("Run")]
    public IEnumerable<StepCase> RunCases => StepCases;

    [AutoTheory.GenerateAsyncTheory("DeserializeAndRun")]
    public IEnumerable<StepCase> DeserializeAndRunCases => StepCases
        .Where(x => x.TestDeserializeAndRun)
        .Select(x => x with { SerializeFirst = true });

    #pragma warning disable CA1034 // Nested types should not be visible
    public record StepCase : CaseThatExecutes
        #pragma warning restore CA1034 // Nested types should not be visible
    {
        public StepCase(
            string name,
            TStep step,
            TOutput expectedOutput,
            params string[] expectedLoggedValues)
            : this(name, step, new ExpectedValueOutput(expectedOutput), expectedLoggedValues) { }

        public StepCase(
            string name,
            IStep<Unit> step,
            Unit _,
            params string[] expectedLoggedValues)
            : this(name, step, ExpectedUnitOutput.Instance, expectedLoggedValues) { }

        protected StepCase(
            string name,
            IStep step,
            ExpectedOutput expectedOutput,
            IReadOnlyCollection<string> expectedLoggedValues) : base(name, expectedLoggedValues)
        {
            Step           = step;
            ExpectedOutput = expectedOutput;
        }

        public bool SerializeFirst { get; set; }

        /// <summary>
        /// The expected output of the step.
        /// If this is not set. It is expected that a unit will be returned
        /// </summary>
        public ExpectedOutput ExpectedOutput { get; }

        public bool TestDeserializeAndRun { get; set; } = true;

        /// <inheritdoc />
        public override string ToString() => Name;

        public IStep Step { get; }

        /// <inheritdoc />
        public override async Task<IStep> GetStepAsync(
            IExternalContext externalContext,
            ITestOutputHelper testOutputHelper)
        {
            await ValueTask.CompletedTask;

            if (!SerializeFirst)
                return Step;

            var scl = Step.Serialize(SerializeOptions.Serialize);

            testOutputHelper.WriteLine("SCL:");
            testOutputHelper.WriteLine("");
            testOutputHelper.WriteLine(scl);

            //we need to get the settings

            var tStepAssembly = Assembly.GetAssembly(typeof(TStep))!;

            var sfs = StepFactoryStoreToUse.GetValueOrDefault(
                StepFactoryStore.TryCreateFromAssemblies(
                        externalContext,
                        tStepAssembly
                    )
                    .GetOrThrow()
            );

            var deserializeResult = SCLParsing.TryParseStep(scl);

            deserializeResult.ShouldBeSuccessful();

            var freezeResult = deserializeResult.Value.TryFreeze(SCLRunner.RootCallerMetadata, sfs);

            freezeResult.ShouldBeSuccessful();

            return freezeResult.Value;
        }

        /// <inheritdoc />
        public override void CheckOutputResult(Result<TOutput, IError> outputResult) =>
            ExpectedOutput.CheckOutputResult(outputResult);

        /// <inheritdoc />
        public override void CheckUnitResult(Result<Unit, IError> result) =>
            ExpectedOutput.CheckUnitResult(result);
    }
}
