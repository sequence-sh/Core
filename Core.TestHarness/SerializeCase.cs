using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoTheory;
using FluentAssertions;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.TestHarness;

public abstract partial class StepTestBase<TStep, TOutput>
{
    [AutoTheory.GenerateAsyncTheory("Serialize")]
    protected virtual IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            var case1 = CreateDefaultSerializeCase();

            yield return case1;
        }
    }

    #pragma warning disable CA1034 // Nested types should not be visible
    public record SerializeCase(
            string Name,
            TStep Step,
            string ExpectedSCL,
            Configuration? ExpectedConfiguration = null) : IAsyncTestInstance
        #pragma warning restore CA1034 // Nested types should not be visible
    {
        /// <inheritdoc />
        public override string ToString() => Name;

        /// <inheritdoc />
        public async Task RunAsync(ITestOutputHelper testOutputHelper)
        {
            var realSCL = SpaceCompressor.CompressNewLines(Step.Serialize().TrimEnd());

            testOutputHelper.WriteLine(realSCL);
            var trueExpected = SpaceCompressor.CompressNewLines(ExpectedSCL.Trim());

            realSCL.Should().Be(trueExpected);

            await Task.CompletedTask;
        }
    }

    public static SerializeCase CreateDefaultSerializeCase()
    {
        var (step, values) = CreateStepWithDefaultOrArbitraryValues();

        var stepName           = new TStep().StepFactory.TypeName;
        var expectedSCLBuilder = new StringBuilder();

        expectedSCLBuilder.Append(stepName);
        expectedSCLBuilder.Append(' ');

        var pairs = values
            .Select(x => $"{x.Key}: {x.Value}");

        expectedSCLBuilder.AppendJoin(" ", pairs);
        var c = new SerializeCase("Default", step, expectedSCLBuilder.ToString().Trim());

        return c;
    }
}
