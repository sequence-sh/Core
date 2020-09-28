using System.Collections.Generic;
using FluentAssertions;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Serialization;
using Reductech.EDR.Processes.Test.Extensions;
using Reductech.EDR.Processes.Util;
using Xunit;
using Xunit.Abstractions;
using ITestCase = Reductech.EDR.Processes.Test.Extensions.ITestCase;

namespace Reductech.EDR.Processes.Test
{
    public class SerializationTests : SerializationTestCases
    {
        public SerializationTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(SerializationTestCases))]
        public override void Test(string key) => base.Test(key);
    }

    public class SerializationTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases
        {
            get
            {
                yield return new SerializationTestCase(
                    new Sequence
                    {
                        Steps = new List<IRunnableProcess<Unit>>
                        {
                            new SetVariable<string>{Value = new Constant<string>("Hello World"), VariableName = new VariableName("Foo")},
                            new SetVariable<string>{Value = new GetVariable<string>(){VariableName = new VariableName("Foo")}, VariableName = new VariableName("Bar")},
                            new Print<string>{Value = new GetVariable<string>(){VariableName = new VariableName("Bar")}}
                        }
                    },
                    @"- <Foo> = 'Hello World'
- <Bar> = <Foo>
- Print(Value = <Bar>)"
                );



                yield return new SerializationTestCase(new Print<int>
                {
                    Value = new ApplyMathOperator
                    {
                        Left = new Constant<int>(2),
                        Operator = new Constant<MathOperator>(MathOperator.Multiply),
                        Right = new Constant<int>(3)
                    }
                }, @"Print(Value = 2 * 3)");

                yield return new SerializationTestCase(new Print<int>
                {
                    Value = new ApplyMathOperator
                    {
                        Left = new Constant<int>(2),
                        Operator = new Constant<MathOperator>(MathOperator.Power),
                        Right = new Constant<int>(3)
                    }
                }, @"Print(Value = 2 ^ 3)");

                yield return new SerializationTestCase(new Print<bool>
                {
                    Value = new Not
                    {
                        Boolean = new Constant<bool>(true)
                    }
                }, @"Print(Value = not True)");

                yield return new SerializationTestCase(new Print<bool>
                {
                    Value = new Compare<int>
                    {
                        Left = new Constant<int>(2),
                        Operator = new Constant<CompareOperator>(CompareOperator.GreaterThanOrEqual),
                        Right = new Constant<int>(3)
                    }
                }, @"Print(Value = 2 >= 3)");

                yield return new SerializationTestCase(new Print<bool>
                {
                    Value = new ApplyBooleanOperator
                    {
                        Left = new Constant<bool>(true),
                        Operator = new Constant<BooleanOperator>(BooleanOperator.And),
                        Right = new Constant<bool>(false)
                    }
                }, @"Print(Value = True && False)");

                yield return new SerializationTestCase(new Print<bool>
                {
                    Value = new ArrayIsEmpty<string>
                    {
                        Array = new Array<string>{Elements = new List<IRunnableProcess<string>>()}
                    }
                }, @"Print(Value = ArrayIsEmpty(Array = Array(Elements = [])))");

                yield return new SerializationTestCase(
                    new Print<string>
                    {
                        Value = new Constant<string>("I have config"),
                        ProcessConfiguration = new ProcessConfiguration()
                        {
                            TargetMachineTags = new List<string>(){"Tag1"},
                            DoNotSplit = false,
                            Priority = 1
                        }
                    }, @"Do: Print
Config:
  AdditionalRequirements: 
  TargetMachineTags:
  - Tag1
  DoNotSplit: false
  Priority: 1
Value: I have config");

            }
        }


        private class SerializationTestCase : ITestCase
        {
            public IRunnableProcess RunnableProcess { get; }
            public string ExpectedYaml { get; }

            public SerializationTestCase(IRunnableProcess runnableProcess, string expectedYaml)
            {
                RunnableProcess = runnableProcess;
                ExpectedYaml = expectedYaml;
            }


            /// <inheritdoc />
            public string Name => RunnableProcess.Name;

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var fp = RunnableProcess.Unfreeze();

                var yaml = fp.SerializeToYaml();

                yaml.Should().Be(ExpectedYaml);
            }
        }
    }
}
