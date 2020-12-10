using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests
{
    public class SerializationTests : SerializationTestCases
    {
        public SerializationTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory(Skip = "true")]
        [ClassData(typeof(SerializationTestCases))]
        public override Task Test(string key) => base.Test(key);
    }

    public class SerializationTestCases : TestBaseParallel
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCaseParallel> TestCases
        {
            get
            {
                yield return new SerializationTestMethod(
                    new Sequence<Unit>
                    {
                        InitialSteps = new List<IStep<Unit>>
                        {
                            SetVariable("Foo", ("Hello World")),
                            new SetVariable<StringStream>{Value = new GetVariable<StringStream> {Variable = new VariableName("Foo")}, Variable = new VariableName("Bar")},
                        },
                        FinalStep = new Print<StringStream>{Value = new GetVariable<StringStream> {Variable = new VariableName("Bar")}}
                    },
                    @"- <Foo> = 'Hello World'
- <Bar> = <Foo>
- Print(Value = <Bar>)"
                );



                yield return new SerializationTestMethod(new Print<int>
                {
                    Value = new ApplyMathOperator
                    {
                        Left = new IntConstant(2),
                        Operator = Constant(MathOperator.Multiply),
                        Right = new IntConstant(3)
                    }
                }, @"Print(Value = (2 * 3))");

                yield return new SerializationTestMethod(new Print<int>
                {
                    Value = new ApplyMathOperator
                    {
                        Left = new IntConstant(2),
                        Operator =  Constant(MathOperator.Multiply),
                        Right = new ApplyMathOperator
                        {
                            Left = new IntConstant(3),
                            Operator = Constant(MathOperator.Add),
                            Right = new IntConstant(4)
                        }
                    }
                }, @"Print(Value = (2 * (3 + 4)))");

                yield return new SerializationTestMethod(new Print<int>
                {
                    Value = new ApplyMathOperator
                    {
                        Left = new ApplyMathOperator
                        {
                            Left = new IntConstant(2),
                            Operator = Constant(MathOperator.Multiply),
                            Right = new IntConstant(3)
                        },
                        Operator = Constant(MathOperator.Add),
                        Right  = new IntConstant(4)
                    }
                }, @"Print(Value = ((2 * 3) + 4))");

                yield return new SerializationTestMethod(new Print<int>
                {
                    Value = new ApplyMathOperator
                    {
                        Left = new IntConstant(2),
                        Operator = Constant(MathOperator.Power),
                        Right = new IntConstant(3)
                    }
                }, @"Print(Value = (2 ^ 3))");

                yield return new SerializationTestMethod(new Print<bool>
                {
                    Value = new Not
                    {
                        Boolean = new BoolConstant(true)
                    }
                }, @"Print(Value = not(True))");

                yield return new SerializationTestMethod(new Print<bool>
                {
                    Value = new Compare<int>
                    {
                        Left = new IntConstant(2),
                        Operator =  Constant(CompareOperator.GreaterThanOrEqual),
                        Right = new IntConstant(3)
                    }
                }, @"Print(Value = (2 >= 3))");

                yield return new SerializationTestMethod(new Print<bool>
                {
                    Value = new ApplyBooleanOperator
                    {
                        Left = new BoolConstant(true),
                        Operator = Constant(BooleanOperator.And),
                        Right = new BoolConstant(false)
                    }
                }, @"Print(Value = (True && False))");


                yield return new SerializationTestMethod(new Print<bool>
                {
                    Value = new ApplyBooleanOperator
                    {
                        Left = new StringIsEmpty{String = new StringConstant("Hello") },
                        Operator = Constant(BooleanOperator.And),
                        Right = new StringIsEmpty{String = new StringConstant("World") }
                    }
                }, @"Print(Value = (StringIsEmpty(String = 'Hello') && StringIsEmpty(String = 'World')))");

                yield return new SerializationTestMethod(new Print<bool>
                {
                    Value = new ApplyBooleanOperator
                    {
                        Left = new Not{Boolean = new BoolConstant(true)},
                        Operator = Constant(BooleanOperator.And),
                        Right = new Not{Boolean = new BoolConstant(false)}
                    }
                }, @"Print(Value = (not(True) && not(False)))");

                yield return new SerializationTestMethod(new Print<bool>
                {
                    Value = new ArrayIsEmpty<StringStream>
                    {
                        Array = new Array<StringStream>{Elements = new List<IStep<StringStream>>()}
                    }
                }, @"Print(Value = ArrayIsEmpty(Array = Array(Elements = [])))");

                yield return new SerializationTestMethod(
                    new Sequence<Unit>
                    {
                        InitialSteps = new List<IStep<Unit>>
                        {
                            new SetVariable<CompareOperator>()
                            {
                                Value = Constant(CompareOperator.LessThan),
                                Variable = new VariableName("Foo")
                            }
                        },
                        FinalStep = new Print<bool>
                        {
                            Value = new Compare<int>
                            {
                                Left = new IntConstant(1),
                                Right = new IntConstant(2),
                                Operator = new GetVariable<CompareOperator>
                                {
                                    Variable = new VariableName("Foo"),
                                    Configuration = new Configuration
                                    {
                                        DoNotSplit = true
                                    }
                                }
                            }
                        }
                    }, @"- <Foo> = CompareOperator.LessThan
- Do: Print
  Value:
    Do: Compare
    Left: 1
    Operator:
      Do: GetVariable
      Config:
        DoNotSplit: true
      Variable: <Foo>
    Right: 2"
                );

                yield return new SerializationTestMethod(
                    new Print<StringStream>
                    {
                        Value = new StringConstant("I have config"),
                        Configuration = new Configuration
                        {
                            TargetMachineTags = new List<string> {"Tag1"},
                            DoNotSplit = false,
                            Priority = 1
                        }
                    }, @"Do: Print
Config:
  TargetMachineTags:
  - Tag1
  DoNotSplit: false
  Priority: 1
Value: 'I have config'");

                yield return new SerializationTestMethod(
                    new Print<StringStream>
                    {
                        Value = new StringConstant("I have config too"),
                        Configuration = new Configuration
                        {
                            TargetMachineTags = new List<string> { "Tag1" },
                            DoNotSplit = false,
                            Priority = 1,
                            AdditionalRequirements = new List<Requirement>
                            {
                                new Requirement
                                {
                                    Name = "ValueIf",
                                    MinVersion = new Version(1,2,3,4),
                                    MaxVersion = new Version(5,6,7,8),
                                    Notes = "ABC123"
                                }
                            }
                        }
                    }, @"Do: Print
Config:
  AdditionalRequirements:
  - Notes: ABC123
    Name: ValueIf
    MinVersion: 1.2.3.4
    MaxVersion: 5.6.7.8
  TargetMachineTags:
  - Tag1
  DoNotSplit: false
  Priority: 1
Value: 'I have config too'");





            }
        }


        private class SerializationTestMethod : ITestBaseCaseParallel
        {
            public IStep Step { get; }
            public string ExpectedText { get; }

            public SerializationTestMethod(IStep step, string expectedText)
            {
                Step = step;
                ExpectedText = expectedText;

                Name = GetName(ExpectedText);
            }

            private static string GetName(string expectedYaml)
            {
                if (expectedYaml.Length <= 40)
                    return expectedYaml;

                return expectedYaml.Substring(0, 25) + "..." + expectedYaml[^10..] + "(" + expectedYaml.Length + ")";
            }

            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public async Task ExecuteAsync(ITestOutputHelper testOutputHelper)
            {
                var text = await Step.SerializeAsync(CancellationToken.None);

                text.Should().Be(ExpectedText);
            }
        }
    }
}
