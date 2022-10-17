using Reductech.Sequence.Core.Internal.Parser;

namespace Reductech.Sequence.Core.TestHarness;

public abstract partial class StepTestBase<TStep, TOutput>
{
    /// <summary>
    /// Deserialization Cases
    /// </summary>
    [GenerateAsyncTheory("Deserialize")]
    protected virtual IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield break;
        }
    }

    /// <summary>
    /// A test that deserializes a step
    /// </summary>
    public record DeserializeCase : CaseThatExecutes
    {
        /// <summary>
        /// Create a new DeserializeCase
        /// </summary>
        public DeserializeCase(
            string name,
            string scl,
            TOutput expectedOutput,
            params string[] expectedLoggedValues) : base(name, expectedLoggedValues)
        {
            SCL            = scl;
            ExpectedOutput = new ExpectedValueOutput(expectedOutput);
        }

        /// <summary>
        /// Create a new DeserializeCase
        /// </summary>
        public DeserializeCase(
            string name,
            string scl,
            // ReSharper disable once UnusedParameter.Local
            Unit _,
            params string[] expectedLoggedValues) : base(name, expectedLoggedValues)
        {
            SCL            = scl;
            ExpectedOutput = ExpectedUnitOutput.Instance;
        }

        /// <summary>
        /// The SCL to deserialize
        /// </summary>
        public string SCL { get; }

        /// <summary>
        /// The expected output of the step
        /// </summary>
        public ExpectedOutput ExpectedOutput { get; }

        /// <inheritdoc />
        public override async Task<IStep> GetStepAsync(
            IExternalContext externalContext,
            ITestOutputHelper testOutputHelper)
        {
            await Task.CompletedTask;

            var tStepAssembly = Assembly.GetAssembly(typeof(TStep))!;

            var sfs = StepFactoryStoreToUse.GetValueOrDefault(
                StepFactoryStore.TryCreateFromAssemblies(
                        externalContext,
                        tStepAssembly
                    )
                    .GetOrThrow()
            );

            testOutputHelper.WriteLine(SCL);

            var deserializeResult = SCLParsing.TryParseStep(SCL);

            deserializeResult.ShouldBeSuccessful();

            var freezeResult = deserializeResult.Value.TryFreeze(
                SCLRunner.RootCallerMetadata,
                sfs,
                new OptimizationSettings(true, null)
            );

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
