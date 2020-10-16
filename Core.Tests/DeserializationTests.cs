using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;


namespace Reductech.EDR.Core.Tests
{
    public class DeserializationTests : DeserializationTestCases
    {
        public DeserializationTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;


        /// <inheritdoc />
        [Theory(Timeout = 10000)]
        [ClassData(typeof(DeserializationTestCases))]
        public override void Test(string key) => base.Test(key);
    }

    public class DeserializationTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCase> TestCases
        {
            get
            {
                yield return new DeserializationTestFunction(
                    @"- <Foo> = 'Hello World'
- <Bar> = <Foo>
- Print(Value = <Bar>)", "Hello World");

                yield return new DeserializationTestFunction(@"Print(Value = 'Mark''s string')", "Mark's string" );
                yield return new DeserializationTestFunction(@"Print(Value = ""Mark's string"")", "Mark's string" );


                yield return new DeserializationTestFunction(@"Print(Value = 2 * 3)", 6);

                yield return new DeserializationTestFunction(@"Print(Value = 3 - 2)", 1);

                yield return new DeserializationTestFunction(@"print(value = 2 * 3)", 6);

                yield return new DeserializationTestFunction(@"print(value = 6 / 3)", 2);

                yield return new DeserializationTestFunction(@"print(value = 6 ^ 2)", 36);

                yield return new DeserializationTestFunction(@"print(value = 7 % 2)", 1);

                yield return new DeserializationTestFunction(@"print(value = 7 modulo 2)", 1);

                yield return new DeserializationTestFunction(@"do: Print
Value: falsetto", "falsetto"); //check 'false' delimiter

                yield return new DeserializationTestFunction(@"do: Print
Value: notable", "notable");//check 'not' delimiter

                yield return new DeserializationTestFunction(@"print(value=2*3)", 6);

                yield return new DeserializationTestFunction(@"Print(Value = 2 ^ 3)", 8);

                yield return new DeserializationTestFunction(@"Print(Value = not (True))", false);


                yield return new DeserializationTestFunction(@"Print(Value = 2 >= 3)", false);
                yield return new DeserializationTestFunction(@"Print(Value = 4 >= 3)", true);
                yield return new DeserializationTestFunction(@"Print(Value = 3 >= 3)", true);

                yield return new DeserializationTestFunction(@"Print(Value = 3 > 3)", false);
                yield return new DeserializationTestFunction(@"Print(Value = 4 > 3)", true);
                yield return new DeserializationTestFunction(@"Print(Value = 3 < 3)", false);

                yield return new DeserializationTestFunction(@"Print(Value = 3 <= 3)", true);

                yield return new DeserializationTestFunction(@"Print(Value = 2 * (3 + 4))",14);
                yield return new DeserializationTestFunction(@"Print(Value = (2 * 3) + 4)",10);

                yield return new DeserializationTestFunction(@"Print(Value = (2 >= 3))", false);

                yield return new DeserializationTestFunction(@"Print(Value = (2 * (3 + 4)))", 14);
                yield return new DeserializationTestFunction(@"Print(Value = (2*(3+4)))", 14);
                yield return new DeserializationTestFunction(@"Print(Value = ((2 * 3) + 4))", 10);

                yield return new DeserializationTestFunction(@"Print(Value = True && False)", false);

                yield return new DeserializationTestFunction(@"Print(Value = StringIsEmpty(String = 'Hello') && StringIsEmpty(String = 'World'))", false);

                yield return new DeserializationTestFunction(@"Print(Value = not (True) && not(False))", false);

                yield return new DeserializationTestFunction(@"Print(Value = true && false)", false);

                yield return new DeserializationTestFunction(@"Print(Value = true and false)", false);

                yield return new DeserializationTestFunction("Print(Value = ArrayIsEmpty(Array = Array(Elements = [])))", true);


                yield return new DeserializationTestFunction(
                    @"Do: Print
Config:
  AdditionalRequirements: 
  TargetMachineTags:
  - Tag1
  DoNotSplit: false
  Priority: 1
Value: I have config", "I have config"
                )
                {
                    ExpectedConfiguration = new Configuration
                    {
                        TargetMachineTags = new List<string> {"Tag1"},
                        DoNotSplit = false,
                        Priority = 1
                    }
                };

                yield return new DeserializationTestFunction(@"Do: Print
Config:
  AdditionalRequirements:
  - Notes: ABC123
    Name: Test
    MinVersion: 1.2.3.4
    MaxVersion: 5.6.7.8
  TargetMachineTags:
  - Tag1
  DoNotSplit: false
  Priority: 1
Value: I have config too", "I have config too")
                {
                    ExpectedConfiguration = new Configuration
                    {
                        TargetMachineTags = new List<string> { "Tag1" },
                        DoNotSplit = false,
                        Priority = 1,
                        AdditionalRequirements = new List<Requirement>
                        {
                            new Requirement
                            {
                                MaxVersion = new Version(5,6,7,8),
                                MinVersion = new Version(1,2,3,4),
                                Name = "Test",
                                Notes = "ABC123"
                            }
                        }
                    }
                }

                    ;

            }
        }


        private class DeserializationTestFunction : ITestBaseCase
        {

            public DeserializationTestFunction(string yaml, params object[] expectedLoggedValues)
            {
                Yaml = yaml;
                ExpectedLoggedValues = expectedLoggedValues.Select(x => x.ToString()!).ToList();
            }

            /// <inheritdoc />
            public string Name => Yaml;

            private string Yaml { get; }

            public Configuration? ExpectedConfiguration { get; set; } = null!;

            private IReadOnlyCollection<string> ExpectedLoggedValues { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var pfs = StepFactoryStore.CreateUsingReflection(typeof(StepFactory));
                var logger = new TestLogger();

                var deserializeResult = YamlMethods.DeserializeFromYaml(Yaml, pfs);

                deserializeResult.ShouldBeSuccessful();

                var freezeResult = deserializeResult.Value.TryFreeze().BindCast<IStep, IStep<Unit>>();

                freezeResult.ShouldBeSuccessful();

                var runResult = freezeResult.Value
                    .Run(new StateMonad(logger, EmptySettings.Instance, ExternalProcessRunner.Instance), CancellationToken.None).Result;

                runResult.ShouldBeSuccessful(x => x.AsString);

                logger.LoggedValues.Should().BeEquivalentTo(ExpectedLoggedValues);

                if (ExpectedConfiguration != null || freezeResult.Value.Configuration != null)
                {
                    freezeResult.Value.Configuration.Should().BeEquivalentTo(ExpectedConfiguration);
                }
            }
        }
    }
}
