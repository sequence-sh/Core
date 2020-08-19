using System.Collections.Generic;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Processes.Serialization;
using Reductech.EDR.Processes.Test.Extensions;
using Xunit;
using Xunit.Abstractions;
using ITestCase = Reductech.EDR.Processes.Test.Extensions.ITestCase;

namespace Reductech.EDR.Processes.Test
{
    public class YamlTests : YamlTestCases
    {
        public YamlTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(YamlTestCases))]
        public override void Test(string key) => base.Test(key);
    }


    public class YamlTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases
        {
            get
            {
                yield return new TestCase(
@"- <Foo> = 'Hello World'
- <Bar> = <Foo>
- Print <Bar>", "Hello World");
            }
        }


        private sealed class TestCase : ITestCase
        {
            public TestCase(string yaml, params string[] expectedLoggedValues)
            {
                ExpectedLoggedValues = expectedLoggedValues;
                Yaml = yaml;
            }

            /// <inheritdoc />
            public string Name => Yaml;

            /// <summary>
            /// The yaml to test
            /// </summary>
            public string Yaml { get; }


            public IReadOnlyList<string> ExpectedLoggedValues { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var pfs = ProcessFactoryStore.CreateUsingReflection(typeof(RunnableProcessFactory));
                var logger = new TestLogger();

                var freezeResult = YamlHelper.DeserializeFromYaml(Yaml, pfs)
                    .Bind(x => x.TryFreeze())
                    .BindCast<IRunnableProcess, IRunnableProcess<Unit>>();

                freezeResult.ShouldBeSuccessful();

                var runResult = freezeResult.Value.Run(new ProcessState(logger, EmptySettings.Instance));

                runResult.ShouldBeSuccessful();

                logger.LoggedValues.Should().BeEquivalentTo(ExpectedLoggedValues);

                var newYaml = freezeResult.Value.Unfreeze().SerializeToYaml();

                newYaml.Trim().Should().Be(Yaml);
            }
        }
    }
}