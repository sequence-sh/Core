using System.Collections.Generic;
using FluentAssertions;
using Reductech.EDR.Core.General;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Tests.Extensions;
using Reductech.EDR.Core.Util;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests
{

    public class StepTest : StepTestCases
    {
        public StepTest(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(StepTestCases))]
        public override void Test(string key) => base.Test(key);
    }

    public class StepTestCases : TestBase
    {

        public const string HelloWorldString = "Hello World";
        public static readonly VariableName FooString = new VariableName("Foo");
        public static readonly VariableName BarString = new VariableName("Bar");

        /// <inheritdoc />
        protected override IEnumerable<ITestFunction> TestCases
        {
            get
            {
                yield return new TestFunction("Print 'Hello World'", Print (Constant(HelloWorldString)), HelloWorldString);

                yield return new TestFunction("Print 'Mark's string'", Print (Constant("Mark's string")), "Mark's string");


                yield return new TestFunction("<Foo> = 'Hello World'; Print <Foo>",Sequence
                (
                    SetVariable(FooString, Constant(HelloWorldString)),
                    Print(GetVariable<string>(FooString))), HelloWorldString);

                yield return new TestFunction("<Foo> = 'Hello World'; <Bar> = <Foo>; Print <Bar>", Sequence(SetVariable(FooString, Constant(HelloWorldString)),
                    SetVariable(BarString, GetVariable<string>(FooString)),
                    Print(GetVariable<string>(BarString))), HelloWorldString);


                yield return new TestFunction("<Foo> = 1 < 2; Print <Foo>",Sequence(
                    SetVariable(FooString, new Compare<int>
                    {
                        Left = Constant(1),
                        Operator = Constant(CompareOperator.LessThan),
                        Right = Constant(2)
                    }),
                    Print(GetVariable<bool>(FooString))

                ), true.ToString());

                yield return new TestFunction("Print +",
                    new Print<MathOperator>
                    {
                        Value = new Constant<MathOperator>(MathOperator.Add)
                    }, MathOperator.Add.ToString());


                //yield return new TestFunction("Print 'True And Not False'",
                //    Print(new ApplyBooleanOperator
                //    {
                //        Left = Constant(true),
                //        Right = new Not { Boolean = Constant(false) },
                //        Operator = Constant(BooleanOperator.And)
                //    }), true.ToString());

                //yield return new TestFunction("Print 'False Or Not False'",
                //    Print(new ApplyBooleanOperator
                //    {
                //        Left = Constant(false),
                //        Right = new Not { Boolean = Constant(false) },
                //        Operator = Constant(BooleanOperator.Or)
                //    }), true.ToString());

                yield return new TestFunction("Foreach <Foo> in ['Hello'; 'World']; Print <Foo>",
                    new ForEach<string>
                    {
                        Action = Print(GetVariable<string>(FooString)),
                        Array = Array(Constant("Hello"),
                            Constant("World")),
                        VariableName = FooString
                    }, "Hello", "World");

                yield return new TestFunction("If True then Print 'Hello World' else Print 'World Hello'",
                    new Conditional
                    {
                        Condition = Constant(true),
                        ThenStep = Print(Constant(HelloWorldString)),
                        ElseStep = Print(Constant("World Hello"))
                    },
                    HelloWorldString);


                yield return new TestFunction("For <Foo> = 5; <Foo> <= 10; += 2; Print <Foo>",
                    new For
                    {
                        VariableName = FooString,
                        Action = Print(GetVariable<int>(FooString)),
                        From = Constant(5),
                        To = Constant(10),
                        Increment = Constant(2)
                    },
                    "5", "7", "9");

                yield return new TestFunction("<Foo> = True; Repeat 'Print 'Hello World'; <Foo> = False' while '<Foo>'",
                    Sequence(SetVariable(FooString, Constant(true)),
                        new RepeatWhile
                        {
                            Action = Sequence(Print(Constant(HelloWorldString)),
                                SetVariable(FooString, Constant(false))),
                            Condition = GetVariable<bool>(FooString)
                        }),
                    HelloWorldString);

                yield return new TestFunction("Print ApplyMathOperator(Left: 2, Operator: *, Right: 3)",
                    Print(new ApplyMathOperator
                    {
                        Left = Constant(2),
                        Right = Constant(3),
                        Operator = Constant(MathOperator.Multiply)
                    }), "6");

                yield return new TestFunction("Print ArrayCount(Array: ['Hello'; 'World'])",
                    Print(new ArrayCount<string>
                    {
                        Array = Array(Constant("Hello"),
                            Constant("World"))
                    }),
                    "2"
                );

                yield return new TestFunction("Print ArrayIsEmpty(Array: [])",
                    Print(new ArrayIsEmpty<string>{Array = Array<string>()}), true.ToString());

                yield return new TestFunction("Print ArrayIsEmpty(Array: ['Hello World'])",
                    Print(new ArrayIsEmpty<string>
                    {
                        Array = Array(Constant(HelloWorldString))
                    }), false.ToString());

                yield return new TestFunction("Print Length of 'Hello World'",
                    Print(new LengthOfString
                    {
                        String = Constant(HelloWorldString)
                    }), "11");

                yield return new TestFunction("Print '''' is empty?",
                    Print(new StringIsEmpty
                    {
                        String = Constant("")
                    }),true.ToString()
                );

                yield return new TestFunction("Print ''Hello World'' is empty?",
                    Print(new StringIsEmpty
                    {
                        String = Constant(HelloWorldString)
                    }), false.ToString()
                );

                yield return new TestFunction("Print FirstIndexOfElement(Array: ['Hello'; 'World'], Element: 'World')",
                    Print(new FirstIndexOfElement<string>
                    {
                        Array = Array(Constant("Hello"), Constant("World")),
                        Element = Constant("World")
                    }),
                    1.ToString()
                );

                yield return new TestFunction("Print FirstIndexOfElement(Array: ['Hello'; 'World'], Element: 'Goodbye')",
                    Print(new FirstIndexOfElement<string>
                    {
                        Array = Array(Constant("Hello"), Constant("World")),
                        Element = Constant("Goodbye")
                    }),
                    (-1).ToString()
                );

                yield return new TestFunction("Print Join Repeat(Element: 'Hello', Number: 3)",Print(new JoinStrings
                {
                    Delimiter = Constant(", "),
                    List = new Repeat<string>
                    {
                        Number = Constant(3),
                        Element = Constant("Hello")
                    }
                }), "Hello, Hello, Hello");

                yield return new TestFunction("Print ElementAtIndex(Array: SplitString(Delimiter: ', ', String: 'Hello, World'), Index: 1)",
                    Print(
                        new ElementAtIndex<string>
                        {
                            Array = new SplitString
                            {
                                Delimiter = Constant(", ") ,
                                String = Constant("Hello, World")
                            },
                            Index = Constant(1)
                        }),
                    "World");

                yield return new TestFunction("<Foo> = 2; IncrementVariable(Amount: 3, Variable: <Foo>); Print <Foo>",
                    Sequence(SetVariable(FooString, Constant(2)),
                        new IncrementVariable
                        {
                            Amount = Constant(3),
                            Variable = FooString
                        },
                        Print(GetVariable<int>(FooString))
                    ),

                    5.ToString());

                yield return new TestFunction("Print ToCase(Case: Upper, String: 'Hello World')", Print(new ToCase{Case = Constant(TextCase.Upper), String = Constant(HelloWorldString)}),"HELLO WORLD");
                yield return new TestFunction("Print ToCase(Case: Lower, String: 'Hello World')", Print(new ToCase{Case = Constant(TextCase.Lower), String = Constant(HelloWorldString)}),"hello world");
                yield return new TestFunction("Print ToCase(Case: Title, String: 'Hello World')", Print(new ToCase{Case = Constant(TextCase.Title), String = Constant(HelloWorldString)}),"Hello World");


                yield return new TestFunction("Print Trim(Side: Left, String: '  Hello World  ')", Print(new Trim {Side = Constant(TrimSide.Left), String = Constant("  Hello World  ")}), "Hello World  ");
                yield return new TestFunction("Print Trim(Side: Right, String: '  Hello World  ')", Print(new Trim {Side = Constant(TrimSide.Right), String = Constant("  Hello World  ")}), "  Hello World");
                yield return new TestFunction("Print Trim(Side: Both, String: '  Hello World  ')", Print(new Trim {Side = Constant(TrimSide.Both), String = Constant("  Hello World  ")}), HelloWorldString);


                yield return new TestFunction("Print Test(Condition: True, ElseValue: 'World', ThenValue: 'Hello')", Print(new Test<string>
                {
                    Condition = Constant(true),
                    ThenValue = Constant("Hello"),
                    ElseValue = Constant("World")
                }), "Hello");


                yield return new TestFunction("Print Test(Condition: False, ElseValue: 'World', ThenValue: 'Hello')", Print(new Test<string>
                {
                    Condition = Constant(false),
                    ThenValue = Constant("Hello"),
                    ElseValue = Constant("World")
                }), "World");

                yield return new TestFunction("Print Join SortArray(Array: ['B'; 'C'; 'A'], Order: Ascending)",
                    Print(new JoinStrings
                    {
                        Delimiter = Constant(", "),
                        List = new SortArray<string>
                        {
                            Array = Array(Constant("B"), Constant("C"), Constant("A")),
                            Order = Constant(SortOrder.Ascending)
                        }
                    }), "A, B, C");

                yield return new TestFunction("Print Join SortArray(Array: ['B'; 'C'; 'A'], Order: Descending)",
                    Print(new JoinStrings
                    {
                        Delimiter = Constant(", "),
                        List = new SortArray<string>
                        {
                            Array = Array(Constant("B"), Constant("C"), Constant("A")),
                            Order = Constant(SortOrder.Descending)
                        }
                    }), "C, B, A");

                yield return new TestFunction("Print First index of ''World'' in ''Hello World, Goodbye World''",
                    Print(new FirstIndexOf
                    {
                        String = Constant("Hello World, Goodbye World"),
                        SubString = Constant("World" )
                    }),
                    "6"
                );

                yield return new TestFunction("Print Last index of ''World'' in ''Hello World, Goodbye World''",
                    Print(new LastIndexOf
                    {
                        String = Constant("Hello World, Goodbye World"),
                        SubString = Constant("World")
                    }),
                    "21"
                );

                yield return new TestFunction("Print Get character at index '1' in ''Hello World''",
                    Print(new GetLetterAtIndex
                    {
                        Index = Constant(1),
                        String = Constant(HelloWorldString)

                    }), "e");

                yield return new TestFunction("Repeat 'Print 'Hello World'' '3' times.",
                    new RepeatXTimes
                    {
                        Number = Constant(3),
                        Action = Print(Constant(HelloWorldString))
                    },HelloWorldString, HelloWorldString, HelloWorldString);


                yield return new TestFunction("<Foo> = 'Hello'; Append ' World' to <Foo>; Print <Foo>",
                    Sequence(
                        SetVariable(FooString,Constant("Hello")),
                        new AppendString{Variable = FooString, String = Constant(" World") },
                        Print(GetVariable<string>(FooString))
                    ), HelloWorldString);

                yield return new TestFunction("Print GetSubstring(Index: 6, Length: 2, String: 'Hello World')",
                    Print(new GetSubstring
                    {
                        String = Constant(HelloWorldString),
                        Index = Constant(6),
                        Length = Constant(2)
                    }), "Wo");

                yield return new TestFunction("Print ElementAtIndex(Array: ['Hello'; 'World'], Index: 1)",
                    Print(new ElementAtIndex<string>
                    {
                        Array = Array(Constant("Hello"), Constant("World")),
                        Index = Constant(1)
                    }), "World"
                );

                yield return new TestFunction("Print 'I have config'", new Print<string>()
                {
                    Value = Constant("I have config"),
                    Configuration = new Configuration()
                    {
                        Priority = 1,
                        TargetMachineTags = new List<string>()
                        {
                            "Tag1"
                        }
                    }
                }, "I have config");

                //yield return new TestFunction("Print 'I have more config'", new Print<string>()
                //{
                //    Value = Constant("I have more config"),
                //    Configuration = new Configuration
                //    {
                //        Priority = 1,
                //        TargetMachineTags = new List<string>
                //        {
                //            "Tag1",
                //            "Tag2"
                //        },
                //        DoNotSplit = true,
                //        AdditionalRequirements = new List<Requirement>
                //        {
                //            new Requirement
                //            {
                //                MinVersion = new Version(1,0),
                //                MaxVersion = new Version(2,0),
                //                Name = "Test",
                //                Notes = "ABC123"
                //            }
                //        }
                //    }
                //}, "I have more config");

                yield return new TestFunction("AssertTrue(Test: True)", new AssertTrue
                {
                    Test = Constant(true)
                });

                yield return new TestFunction("AssertError(Test: AssertTrue(Test: False))", new AssertError
                {
                    Test = new AssertTrue
                    {
                        Test = Constant(false)
                    }
                });

            }
        }

        private static GetVariable<T> GetVariable<T>(VariableName variableName) => new GetVariable<T>{VariableName = variableName};

        private static Constant<T> Constant<T>(T element) => new Constant<T>(element);

        private static Print<T> Print<T>(IStep<T> element) => new Print<T>{Value = element};

        private static Array<T> Array<T>(params IStep<T>[] elements)=> new Array<T>{Elements = elements};
        private static SetVariable<T> SetVariable<T>(VariableName variableName, IStep<T> step) =>
            new SetVariable<T>
            {
                VariableName = variableName,
                Value = step
            };

        private static Sequence Sequence(params IStep<Unit>[] steps)=> new Sequence{Steps = steps};


        private class TestFunction : ITestFunction
        {
            public TestFunction(string expectedName, IStep step, params string[] expectedLoggedValues)
            {
                Step = step;
                ExpectedLoggedValues = expectedLoggedValues;
                ExpectedName = expectedName;
            }

            public string ExpectedName { get; }

            /// <inheritdoc />
            public string Name => ExpectedName;

            public IStep Step { get; }


            public IReadOnlyList<string> ExpectedLoggedValues { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper outputHelper)
            {
                //Arrange
                var pfs = StepFactoryStore.CreateUsingReflection(typeof(StepFactory));
                var logger = new TestLogger();
                var yamlRunner = new YamlRunner(EmptySettings.Instance, logger, pfs);

                //Act
                var unfrozen = Step.Unfreeze();
                var yaml = unfrozen.SerializeToYaml();
                outputHelper.WriteLine(yaml);
                var runResult = yamlRunner.RunSequenceFromYamlString(yaml);

                //Assert
                runResult.ShouldBeSuccessful();
                logger.LoggedValues.Should().BeEquivalentTo(ExpectedLoggedValues);
                Step.Name.Should().Be(ExpectedName);

            }
        }
    }
}