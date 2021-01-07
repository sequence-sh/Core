using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.TestHarness
{

public abstract partial class StepTestBase<TStep, TOutput>
{
    protected virtual IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            var case1 = CreateDefaultSerializeCase().Result; //TODO fix the synchronicity

            yield return case1;
        }
    }

    public IEnumerable<object?[]> SerializeCaseNames =>
        SerializeCases.Select(x => new[] { x.Name });

    [Theory]
    [NonStaticMemberData(nameof(SerializeCaseNames), true)]
    public async Task Should_serialize_as_expected(string serializeCaseName)
    {
        await SerializeCases.FindAndRunAsync(serializeCaseName, TestOutputHelper);
    }

    #pragma warning disable CA1034 // Nested types should not be visible
    public class SerializeCase : ICase
        #pragma warning restore CA1034 // Nested types should not be visible
    {
        public SerializeCase(
            string name,
            TStep step,
            string expectedSCL,
            Configuration? expectedConfiguration = null)
        {
            Name                  = name;
            Step                  = step;
            ExpectedSCL           = expectedSCL;
            ExpectedConfiguration = expectedConfiguration;
        }

        public string Name { get; }

        /// <inheritdoc />
        public override string ToString() => Name;

        public TStep Step { get; }
        public string ExpectedSCL { get; }
        public Configuration? ExpectedConfiguration { get; }

        /// <inheritdoc />
        public async Task RunCaseAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
        {
            var realSCL = Step.Serialize();

            testOutputHelper.WriteLine(realSCL);

            realSCL.Should().Be(ExpectedSCL);

            await Task.CompletedTask;
        }
    }

    public static async Task<SerializeCase> CreateDefaultSerializeCase()
    {
        var (step, values) = await CreateStepWithDefaultOrArbitraryValuesAsync();

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

}
