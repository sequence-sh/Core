using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using MELT;
using Reductech.EDR.Core.Attributes;
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

        if (!data.Any())
            data.Add("Skip");

        return data;
    }

    [Theory]
    [MemberData(nameof(GetExampleCases))]
    public async Task TestExamples(string scl)
    {
        if (scl == "Skip")
            return; //Skip

        var tohProperty = GetType().GetProperty(nameof(TestOutputHelper))!;

        var testOutputHelper =
            (ITestOutputHelper)tohProperty.GetValue(this)!; //Very hacky way of doing this

        var attribute = typeof(TStep).GetCustomAttributes<SCLExampleAttribute>()
            .Single(x => x.SCL == scl);

        var exampleCase = new ExampleCase(attribute);

        await exampleCase.RunAsync(testOutputHelper);
    }

    public record ExampleCase : CaseThatExecutes
    {
        public ExampleCase(SCLExampleAttribute sclExampleAttribute) : base(
            sclExampleAttribute.SCL,
            sclExampleAttribute.ExpectedLogs ?? new string[] { }
        )
        {
            SCLExampleAttribute = sclExampleAttribute;
            IgnoreFinalState    = true;
        }

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
        public override bool ShouldExecute => SCLExampleAttribute.ExecuteInTests;

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
                    actualString =
                        CompressSpaces(SerializationMethods.SerializeObject(result.Value));
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

        public SCLExampleAttribute SCLExampleAttribute { get; init; }
    }
}

}
