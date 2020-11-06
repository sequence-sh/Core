using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                yield return CreateDefaultSerializeCase(true);
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


        public static SerializeCase CreateDefaultSerializeCase(bool shortForm)
        {
            var (step, values) = CreateStepWithDefaultOrArbitraryValues();


            var stepName = new TStep().StepFactory.TypeName;
            var expectedYamlBuilder = new StringBuilder();

            if (shortForm)
            {
                expectedYamlBuilder.Append(stepName);
                expectedYamlBuilder.Append("(");

                var pairs = values.OrderBy(x => x.Key).Select(x => $"{x.Key} = {x.Value}");

                expectedYamlBuilder.AppendJoin(", ", pairs);
                expectedYamlBuilder.Append(")");
                var c = new SerializeCase("Default", step, expectedYamlBuilder.ToString());

                return c;
            }
            else
            {
                expectedYamlBuilder.AppendLine($"Do: {stepName}");

                var pairs = values.OrderBy(x => x.Key);

                foreach (var pair in pairs)
                {
                    if (pair.Value.Contains("\n"))
                    {
                        expectedYamlBuilder.AppendLine($"{pair.Key}: ");
                        expectedYamlBuilder.AppendLine($"{pair.Value}");
                    }
                    else
                        expectedYamlBuilder.AppendLine($"{pair.Key}: {pair.Value}");
                }

                var c = new SerializeCase("Long Form", step, expectedYamlBuilder.ToString().Trim());
                return c;
            }
        }
    }
}