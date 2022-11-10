using System.Text;

namespace Sequence.Core.TestHarness;

public abstract partial class StepTestBase<TStep, TOutput>
{
    /// <summary>
    /// Tests that test step serialization
    /// </summary>
    [GenerateAsyncTheory("Serialize")]
    protected virtual IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            var case1 = CreateDefaultSerializeCase();

            yield return case1;
        }
    }

    /// <summary>
    /// A test case that tests step serialization
    /// </summary>
    public record SerializeCase(
        string Name,
        TStep Step,
        string ExpectedSCL,
        Configuration? ExpectedConfiguration = null) : IAsyncTestInstance
    {
        /// <inheritdoc />
        public override string ToString() => Name;

        /// <inheritdoc />
        public async Task RunAsync(ITestOutputHelper testOutputHelper)
        {
            var realSCL =
                SpaceCompressor.CompressNewLines(
                    Step.Serialize(SerializeOptions.Serialize).TrimEnd()
                );

            testOutputHelper.WriteLine(realSCL);
            var trueExpected = SpaceCompressor.CompressNewLines(ExpectedSCL.Trim());

            realSCL.Should().Be(trueExpected);

            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// The default serialize case
    /// </summary>
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
