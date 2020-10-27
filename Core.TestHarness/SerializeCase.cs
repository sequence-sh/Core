using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace Core.TestHarness
{
    public abstract partial class StepTestBase<TStep, TOutput>
    {
        protected abstract IEnumerable<SerializeCase> SerializeCases { get; }

        public IEnumerable<object?[]> SerializeCaseNames => SerializeCases.Select(x => new[] { x.Name });

        [Theory]
        [NonStaticMemberData(nameof(SerializeCaseNames), true)]
        public async Task Should_serialize_as_expected(string serializeCaseName)
        {
            await SerializeCases.FindAndRunAsync(serializeCaseName, TestOutputHelper);
        }

        public class SerializeCase : ICase
        {
            public SerializeCase(string name, TStep step, string expectedYaml,
                Configuration? expectedConfiguration = null)
            {
                Name = name;
                Step = step;
                ExpectedYaml = expectedYaml;
                ExpectedConfiguration = expectedConfiguration;
            }

            public string Name { get; }

            /// <inheritdoc />
            public override string ToString() => Name;

            public TStep Step { get; }
            public string ExpectedYaml { get; }
            public Configuration? ExpectedConfiguration { get; }

            /// <inheritdoc />
            public async Task RunCaseAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
            {
                var realYaml = Step.Unfreeze().SerializeToYaml();
                realYaml.Should().Be(ExpectedYaml);

                await Task.CompletedTask;
            }
        }
    }
}