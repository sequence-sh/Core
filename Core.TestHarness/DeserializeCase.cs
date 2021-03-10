using System.Collections.Generic;
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
    [AutoTheory.GenerateAsyncTheory("Deserialize")]
    protected virtual IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield break;
        }
    }

    #pragma warning disable CA1034 // Nested types should not be visible
    public record DeserializeCase : CaseThatExecutes
        #pragma warning restore CA1034 // Nested types should not be visible
    {
        public DeserializeCase(
            string name,
            string scl,
            TOutput expectedOutput,
            params string[] expectedLoggedValues) : base(name, expectedLoggedValues)
        {
            SCL            = scl;
            ExpectedOutput = expectedOutput;
        }

        public DeserializeCase(
            string name,
            string scl,
            Unit expectedOutput,
            params string[] expectedLoggedValues) : base(name, expectedLoggedValues)
        {
            SCL            = scl;
            ExpectedOutput = new ExpectedOutput(expectedOutput);
        }

        public string SCL { get; }
        public ExpectedOutput ExpectedOutput { get; }

        /// <inheritdoc />
        public override async Task<IStep> GetStepAsync(ITestOutputHelper testOutputHelper)
        {
            await Task.CompletedTask;

            var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep));

            testOutputHelper.WriteLine(SCL);

            var deserializeResult = SCLParsing.ParseSequence(SCL);

            deserializeResult.ShouldBeSuccessful(x => x.ToString()!);

            var freezeResult = deserializeResult.Value.TryFreeze(TypeReference.Any.Instance, sfs);
            freezeResult.ShouldBeSuccessful(x => x.ToString()!);

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
