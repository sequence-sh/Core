using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Reductech.EDR.Core.Serialization;
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
                yield return CreateDefaultSerializeCase();
            }
        }

        public IEnumerable<object?[]> SerializeCaseNames => SerializeCases.Select(x => new[] { x.Name });

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

                testOutputHelper.WriteLine(realYaml);

                realYaml.Should().Be(ExpectedYaml);

                await Task.CompletedTask;
            }
        }


        public static SerializeCase CreateDefaultSerializeCase()
        {
            var (step, values) = CreateStepWithDefaultOrArbitraryValues();


            var stepName = new TStep().StepFactory.TypeName;

            var expectedYaml = $"{stepName}({string.Join(", ", values.OrderBy(x => x.Key).Select(x => $"{x.Key} = {x.Value}"))})";

            var c = new SerializeCase("Default", step, expectedYaml);

            return c;
        }
    }
}