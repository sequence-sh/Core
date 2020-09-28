using System.Collections.Generic;
using FluentAssertions;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Serialization;
using Reductech.EDR.Processes.Test.Extensions;
using Reductech.EDR.Processes.Util;
using Xunit;
using Xunit.Abstractions;
using ITestCase = Reductech.EDR.Processes.Test.Extensions.ITestCase;

namespace Reductech.EDR.Processes.Test
{
    public class DeserializationTests : DeserializationTestCases
    {
        public DeserializationTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;


        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(DeserializationTestCases))]
        public override void Test(string key) => base.Test(key);
    }

    public class DeserializationTestCases :TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases
        {
            get
            {
                yield return new DeserializationTestCase(
@"- <Foo> = 'Hello World'
- <Bar> = <Foo>
- Print(Value = <Bar>)", "Hello World");

                yield return new DeserializationTestCase(@"Print(Value = 2 * 3)", 6.ToString());

                yield return new DeserializationTestCase(@"print(value = 2 * 3)", 6.ToString());

                yield return new DeserializationTestCase(@"print(value=2*3)", 6.ToString());

                yield return new DeserializationTestCase(@"Print(Value = 2 ^ 3)", 8.ToString());

                yield return new DeserializationTestCase(@"Print(Value = not True)", false.ToString());


                yield return new DeserializationTestCase(@"Print(Value = 2 >= 3)", false.ToString());


                yield return new DeserializationTestCase(@"Print(Value = True && False)", false.ToString());

                yield return new DeserializationTestCase("Print(Value = ArrayIsEmpty(Array = Array(Elements = [])))", true.ToString());


                yield return new DeserializationTestCase(
                    @"Do: Print
Config:
  AdditionalRequirements: 
  TargetMachineTags:
  - Tag1
  DoNotSplit: false
  Priority: 1
Value: I have config", "I have config"
                    );
            }
        }


        private class DeserializationTestCase : ITestCase
        {

            public DeserializationTestCase(string yaml, params string[] expectedLoggedValues)
            {
                Yaml = yaml;
                ExpectedLoggedValues = expectedLoggedValues;
            }

            /// <inheritdoc />
            public string Name => Yaml;

            private string Yaml { get; }

            private IReadOnlyCollection<string> ExpectedLoggedValues { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var pfs = ProcessFactoryStore.CreateUsingReflection(typeof(RunnableProcessFactory));
                var logger = new TestLogger();

                var deserializeResult = YamlMethods.DeserializeFromYaml(Yaml, pfs);

                deserializeResult.ShouldBeSuccessful();

                var freezeResult = deserializeResult.Value.TryFreeze().BindCast<IRunnableProcess, IRunnableProcess<Unit>>();

                freezeResult.ShouldBeSuccessful();

                var runResult = freezeResult.Value.Run(new ProcessState(logger, EmptySettings.Instance, ExternalProcessRunner.Instance));

                runResult.ShouldBeSuccessful(x => x.AsString);

                logger.LoggedValues.Should().BeEquivalentTo(ExpectedLoggedValues);
            }
        }
    }
}
