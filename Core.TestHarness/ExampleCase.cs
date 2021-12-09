using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using MELT;
using Reductech.EDR.Core.Abstractions;
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

namespace Reductech.EDR.Core.TestHarness;

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
            return;

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
            sclExampleAttribute.ExpectedLogs ?? System.Array.Empty<string>()
        )
        {
            SCLExampleAttribute = sclExampleAttribute;
            IgnoreFinalState    = true;
        }

        /// <inheritdoc />
        public override async Task<IStep> GetStepAsync(
            IExternalContext externalContext,
            ITestOutputHelper testOutputHelper)
        {
            string realSCL;

            StringBuilder sclPrefix = new();

            if (SCLExampleAttribute.VariableInjections is not null)
            {
                foreach (var (variableName, value) in SCLExampleAttribute.VariableInjections)
                {
                    var line = $"- <{variableName}> = {value}";
                    sclPrefix.AppendLine(line);
                }

                var suffix = SCLExampleAttribute.SCL;

                if (!suffix.StartsWith('-'))
                    suffix = "- " + suffix;

                realSCL = sclPrefix + suffix;
            }
            else
            {
                realSCL = SCLExampleAttribute.SCL;
            }

            await Task.CompletedTask;
            testOutputHelper.WriteLine("SCL:");
            testOutputHelper.WriteLine(realSCL);

            if (SCLExampleAttribute.Description is not null)
            {
                testOutputHelper.WriteLine("Description:");
                testOutputHelper.WriteLine(SCLExampleAttribute.Description);
            }

            var tStepAssembly = Assembly.GetAssembly(typeof(TStep))!;

            var sfs = StepFactoryStoreToUse.GetValueOrDefault(
                StepFactoryStore.TryCreateFromAssemblies(
                        externalContext,
                        tStepAssembly
                    )
                    .GetOrThrow()
            );

            var deserializeResult = SCLParsing.TryParseStep(realSCL);

            deserializeResult.ShouldBeSuccessful();

            var freezeResult = deserializeResult.Value.TryFreeze(
                SCLRunner.RootCallerMetadata,
                sfs
            );

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
        public override void CheckObjectResult(IStep step, Result<object, IError> result)
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
