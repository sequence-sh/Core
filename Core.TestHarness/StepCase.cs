using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using MELT;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Util;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using static Reductech.EDR.Core.TestHarness.SpaceCompressor;

namespace Reductech.EDR.Core.TestHarness
{

public abstract partial class StepTestBase<TStep, TOutput>
{
    public static TheoryData<string> GetExampleCases()
    {
        var attributes = typeof(TStep).GetCustomAttributes<SCLExampleAttribute>();
        var data       = new TheoryData<string>();

        foreach (var sclExampleAttribute in attributes)
        {
            data.Add(sclExampleAttribute.SCL);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(GetExampleCases))]
    public async Task TestExamples(string scl)
    {
        var tohProperty = GetType().GetProperty(nameof(TestOutputHelper))!;

        var testOutputHelper =
            (ITestOutputHelper)tohProperty.GetValue(this)!; //Very hacky way of doing this

        var attribute = typeof(TStep).GetCustomAttributes<SCLExampleAttribute>()
            .Single(x => x.SCL == scl);

        var exampleCase = new ExampleCase(attribute);

        await exampleCase.RunAsync(testOutputHelper);
    }

    public record ExampleCase(SCLExampleAttribute SCLExampleAttribute) : CaseThatExecutes(
        SCLExampleAttribute.SCL,
        SCLExampleAttribute.ExpectedLogs
    )
    {
        /// <inheritdoc />
        public override async Task<IStep> GetStepAsync(ITestOutputHelper testOutputHelper)
        {
            await Task.CompletedTask;
            testOutputHelper.WriteLine("SCL:");
            testOutputHelper.WriteLine(SCLExampleAttribute.SCL);

            if (SCLExampleAttribute.Description is not null)
            {
                testOutputHelper.WriteLine("Description:");
                testOutputHelper.WriteLine(SCLExampleAttribute.Description);
            }

            var tStepAssembly = Assembly.GetAssembly(typeof(TStep))!;

            var sfs = StepFactoryStoreToUse.Unwrap(
                StepFactoryStore.CreateFromAssemblies(tStepAssembly)
            );

            var deserializeResult = SCLParsing.TryParseStep(SCLExampleAttribute.SCL);

            deserializeResult.ShouldBeSuccessful();

            var freezeResult = deserializeResult.Value.TryFreeze(SCLRunner.RootCallerMetadata, sfs);

            freezeResult.ShouldBeSuccessful();

            return freezeResult.Value;
        }

        /// <inheritdoc />
        public override void CheckUnitResult(Result<Unit, IError> result)
        {
            result.ShouldBeSuccessful();

            if (SCLExampleAttribute.ExpectedOutput is not null)
                result.Value.Should().Be(SCLExampleAttribute.ExpectedOutput);
        }

        /// <inheritdoc />
        public override void CheckOutputResult(Result<TOutput, IError> result)
        {
            result.ShouldBeSuccessful();

            if (SCLExampleAttribute.ExpectedOutput is null)
                result.Value.Should().Be(Unit.Default);
            else
            {
                string actualString;
                var    expectedString = CompressSpaces(SCLExampleAttribute.ExpectedOutput);

                if (result.Value is StringStream ss)
                {
                    actualString = CompressSpaces(ss.GetString());
                }
                else if (result.Value is string s)
                {
                    actualString = CompressSpaces(s);
                }
                else
                {
                    actualString = CompressSpaces(SerializationMethods.GetString(result.Value));
                }

                actualString.Should().Be(expectedString);
            }
        }

        /// <inheritdoc />
        public override void CheckLoggedValues(ITestLoggerFactory loggerFactory)
        {
            if (SCLExampleAttribute.ExpectedLogs is null)
                return;

            base.CheckLoggedValues(loggerFactory);
        }
    }
}

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
        public override async Task<IStep> GetStepAsync(ITestOutputHelper testOutputHelper)
        {
            await ValueTask.CompletedTask;

            if (!SerializeFirst)
                return Step;

            var scl = Step.Serialize();

            testOutputHelper.WriteLine("SCL:");
            testOutputHelper.WriteLine("");
            testOutputHelper.WriteLine(scl);

            //we need to get the settings

            var tStepAssembly = Assembly.GetAssembly(typeof(TStep))!;

            var sfs = StepFactoryStoreToUse.Unwrap(
                StepFactoryStore.CreateFromAssemblies(tStepAssembly)
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

}
