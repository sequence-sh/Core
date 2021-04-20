using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.TestHarness
{

public abstract partial class StepTestBase<TStep, TOutput>
{
    protected abstract IEnumerable<StepCase> StepCases { get; }

    [AutoTheory.GenerateAsyncTheory("Run")]
    public IEnumerable<StepCase> RunCases => StepCases;

    [AutoTheory.GenerateAsyncTheory("DeserializeAndRun")]
    public IEnumerable<StepCase> DeserializeAndRunCases =>
        StepCases.Select(x => x with { SerializeFirst = true });

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

        /// <inheritdoc />
        public override string ToString() => Name;

        public IStep Step { get; }

        /// <inheritdoc />
        public override async Task<IStep> GetStepAsync(ITestOutputHelper testOutputHelper)
        {
            await ValueTask.CompletedTask;

            if (!SerializeFirst)
                return Step;

            var scl = Step.Serialize();

            testOutputHelper.WriteLine("SCL:");
            testOutputHelper.WriteLine("");
            testOutputHelper.WriteLine(scl);

            var sfs = StepFactoryStore.CreateFromAssemblies(Assembly.GetAssembly(typeof(TStep))!);

            var deserializeResult = SCLParsing.TryParseStep(scl);

            deserializeResult.ShouldBeSuccessful();

            var freezeResult = deserializeResult.Value.TryFreeze(TypeReference.Any.Instance, sfs);

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

}
