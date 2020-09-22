using System.Collections.Generic;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Processes.Internal;
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
- Print(Value = <Bar>)", "Hello World");

                yield return new TestCase(@"Print(Value = 2 * 3)", 6.ToString());


                yield return new TestCase(@"Print(Value = 2 ^ 3)", 8.ToString());

                yield return new TestCase(@"Print(Value = not True)", false.ToString());


                yield return new TestCase(@"Print(Value = 2 >= 3)", false.ToString());


                yield return new TestCase(@"Print(Value = True && False)", false.ToString());

                yield return new TestCase("Print(Value = ArrayIsEmpty(Array = Array(Elements = [])))", true.ToString());


                yield return new TestCase(
                    @"
Do: Print
Config:
  AdditionalRequirements:
  TargetMachineTags:
  - Tag1
  DoNotSplit: false
  Priority: 1
Value: I have config", "I have config"
                    );

                yield return new TestCase(
                    @"
Do: Print
Config:
  AdditionalRequirements:
  - Notes: ABC123
    Name: Test
    MinVersion:
      Major: 1
      Minor: 0
      Build: -1
      Revision: -1
      MajorRevision: -1
      MinorRevision: -1
    MaxVersion:
      Major: 2
      Minor: 0
      Build: -1
      Revision: -1
      MajorRevision: -1
      MinorRevision: -1
  TargetMachineTags:
  - Tag1
  - Tag2
  DoNotSplit: true
  Priority: 1
Value: I have more config", "I have more config");


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

                var freezeResult = YamlMethods.DeserializeFromYaml(Yaml, pfs)
                    .Bind(x => x.TryFreeze())
                    .BindCast<IRunnableProcess, IRunnableProcess<Unit>>();

                freezeResult.ShouldBeSuccessful();

                var runResult = freezeResult.Value.Run(new ProcessState(logger, EmptySettings.Instance, ExternalProcessRunner.Instance));

                runResult.ShouldBeSuccessful(x=>x.AsString);

                logger.LoggedValues.Should().BeEquivalentTo(ExpectedLoggedValues);

                var newYaml = freezeResult.Value.Unfreeze().SerializeToYaml();

                newYaml.Trim().Should().Be(Yaml.Trim());
            }
        }
    }
}