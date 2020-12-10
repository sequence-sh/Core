using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;

using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests
{

    public class StepTest : StepTestCases
    {
        public StepTest(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory(Skip = "true")]
        [ClassData(typeof(StepTestCases))]
        public override Task Test(string key) => base.Test(key);
    }

    public class StepTestCases : TestBaseParallel
    {

        public const string HelloWorldString = "Hello World";
        public static readonly VariableName FooVariableName = new VariableName("Foo");
        public static readonly VariableName BarString = new VariableName("Bar");

        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCaseParallel> TestCases
        {
            get
            {
                foreach (var stepTestCase in TestCasesWithoutConfig)
                {
                    yield return stepTestCase;
                    yield return new StepTestCase(stepTestCase.ExpectedName, stepTestCase.Step, stepTestCase.ExpectedLoggedValues.ToArray())
                    {
                        AddConfiguration = true,
                        //IgnoreName = stepTestCase.IgnoreName,
                        IgnoreLoggedValues = stepTestCase.IgnoreLoggedValues
                    };
                }
            }
        }
        private static IEnumerable<StepTestCase> TestCasesWithoutConfig
        {
            get
            {
                yield return new StepTestCase("Print 'Hello World'", Print(Constant(HelloWorldString)),
                    HelloWorldString);

                yield return new StepTestCase("Print 'Mark's string'", Print(Constant("Mark's string")),
                    "Mark's string");


                yield return new StepTestCase("<Foo> = 'Hello World'; Print <Foo>", Sequence
                (
                    SetVariable(FooVariableName, Constant(HelloWorldString)),
                    Print(GetVariable<StringStream>(FooVariableName))), HelloWorldString);

                yield return new StepTestCase("<Foo> = 'Hello World'; <Bar> = <Foo>; Print <Bar>", Sequence(
                    SetVariable(FooVariableName, Constant(HelloWorldString)),
                    SetVariable(BarString, GetVariable<StringStream>(FooVariableName)),
                    Print(GetVariable<StringStream>(BarString))), HelloWorldString);


                yield return new StepTestCase("<Foo> = 1 < 2; Print <Foo>", Sequence(
                    SetVariable(FooVariableName, new Compare<int>
                    {
                        Left = Constant(1),
                        Operator = Constant(CompareOperator.LessThan),
                        Right = Constant(2)
                    }),
                    Print(GetVariable<bool>(FooVariableName))

                ), true.ToString());

                yield return new StepTestCase("Print +",
                    new Print<MathOperator>
                    {
                        Value = Constant(MathOperator.Add)
                    }, MathOperator.Add.ToString());


                yield return new StepTestCase("Print True && Not False",
                    Print(new ApplyBooleanOperator
                    {
                        Left = Constant(true),
                        Right = new Not { Boolean = Constant(false) },
                        Operator = Constant(BooleanOperator.And)
                    }), true.ToString());

                yield return new StepTestCase("Print False || Not False",
                    Print(new ApplyBooleanOperator
                    {
                        Left = Constant(false),
                        Right = new Not { Boolean = Constant(false) },
                        Operator = Constant(BooleanOperator.Or)
                    }), true.ToString());

                yield return new StepTestCase("Foreach <Foo> in ['Hello'; 'World']; Print <Foo>",
                    new ForEach<StringStream>
                    {
                        Action = Print(GetVariable<StringStream>(FooVariableName)),
                        Array = Array("Hello", "World"),
                        Variable = FooVariableName
                    }, "Hello", "World");


                yield return new StepTestCase("Foreach <Foo> in ['Hello'; 'World']; Print 'Farewell'; Print <Foo>",
                    new ForEach<StringStream>
                    {
                        Action = Sequence(Print(Constant("Farewell")),
                                Print(GetVariable<StringStream>(FooVariableName))),
                        Array = Array("Hello", "World"),
                        Variable = FooVariableName
                    }, "Farewell", "Hello", "Farewell", "World");

                yield return new StepTestCase("If True then Print 'Hello World' else Print 'World Hello'",
                    new If
                    {
                        Condition = Constant(true),
                        Then = Print(Constant(HelloWorldString)),
                        Else = Print(Constant("World Hello"))
                    },
                    HelloWorldString);


                yield return new StepTestCase("For i = 5; i <= 10; += 2; Print <i>",
                    new For
                    {
                        Action = Print(GetVariable<int>(VariableName.Index)),
                        From = Constant(5),
                        To = Constant(10),
                        Increment = Constant(2)
                    },
                    "5", "7", "9");

                yield return new StepTestCase("<Foo> = True; Repeat 'Print 'Hello World'; <Foo> = False' while '<Foo>'",
                    Sequence(SetVariable(FooVariableName, Constant(true)),
                        new While
                        {
                            Action = Sequence(Print(Constant(HelloWorldString)),
                                SetVariable(FooVariableName, Constant(false))),
                            Condition = GetVariable<bool>(FooVariableName)
                        }),
                    HelloWorldString);

                yield return new StepTestCase("Print ApplyMathOperator(Left: 2, Operator: *, Right: 3)",
                    Print(new ApplyMathOperator
                    {
                        Left = Constant(2),
                        Right = Constant(3),
                        Operator = Constant(MathOperator.Multiply)
                    }), "6");

                yield return new StepTestCase("Print ArrayLength(Array: ['Hello'; 'World'])",
                    Print(new ArrayLength<StringStream>
                    {
                        Array = Array("Hello", "World")
                    }),
                    "2"
                );

                yield return new StepTestCase("Print ArrayIsEmpty(Array: [])",
                    Print(new ArrayIsEmpty<StringStream> {Array = Array(System.Array.Empty<string>()) }), true.ToString());

                yield return new StepTestCase("Print ArrayIsEmpty(Array: ['Hello World'])",
                    Print(new ArrayIsEmpty<StringStream>
                    {
                        Array = Array(HelloWorldString)
                    }), false.ToString());

                yield return new StepTestCase("Print ArrayLength of 'Hello World'",
                    Print(new StringLength
                    {
                        String = Constant(HelloWorldString)
                    }), "11");

                yield return new StepTestCase("Print '''' is empty?",
                    Print(new StringIsEmpty
                    {
                        String = Constant("")
                    }), true.ToString()
                );

                yield return new StepTestCase("Print ''Hello World'' is empty?",
                    Print(new StringIsEmpty
                    {
                        String = Constant(HelloWorldString)
                    }), false.ToString()
                );

                yield return new StepTestCase("Print FindElement(Array: ['Hello'; 'World'], Element: 'World')",
                    Print(new FindElement<StringStream>
                    {
                        Array = Array("Hello", "World"),
                        Element = Constant("World")
                    }),
                    1.ToString()
                );

                yield return new StepTestCase(
                    "Print FindElement(Array: ['Hello'; 'World'], Element: 'Goodbye')",
                    Print(new FindElement<StringStream>
                    {
                        Array = Array(("Hello"), ("World")),
                        Element = Constant("Goodbye")
                    }),
                    (-1).ToString()
                );

                yield return new StepTestCase("Print Match Repeat(Element: 'Hello', Number: 3)", Print(new StringJoin
                {
                    Delimiter = Constant(", "),
                    Strings = new Repeat<StringStream>
                    {
                        Number = Constant(3),
                        Element = Constant("Hello")
                    }
                }), "Hello, Hello, Hello");

                yield return new StepTestCase(
                    "Print ElementAtIndex(Array: StringSplit(Delimiter: ', ', String: 'Hello, World'), Index: 1)",
                    Print(
                        new ElementAtIndex<StringStream>
                        {
                            Array = new StringSplit
                            {
                                Delimiter = Constant(", "),
                                String = Constant("Hello, World")
                            },
                            Index = Constant(1)
                        }),
                    "World");

                yield return new StepTestCase("<Foo> = 2; IncrementVariable(Amount: 3, Variable: <Foo>); Print <Foo>",
                    Sequence(SetVariable(FooVariableName, Constant(2)),
                        new IncrementVariable
                        {
                            Amount = Constant(3),
                            Variable = FooVariableName
                        },
                        Print(GetVariable<int>(FooVariableName))
                    ),

                    5.ToString());

                yield return new StepTestCase("Print StringToCase(Case: Upper, String: 'Hello World')",
                    Print(new StringToCase {Case = Constant(TextCase.Upper), String = Constant(HelloWorldString)}),
                    "HELLO WORLD");
                yield return new StepTestCase("Print StringToCase(Case: Lower, String: 'Hello World')",
                    Print(new StringToCase {Case = Constant(TextCase.Lower), String = Constant(HelloWorldString)}),
                    "hello world");
                yield return new StepTestCase("Print StringToCase(Case: Title, String: 'Hello World')",
                    Print(new StringToCase {Case = Constant(TextCase.Title), String = Constant(HelloWorldString)}),
                    "Hello World");


                yield return new StepTestCase("Print StringTrim(Side: Start, String: '  Hello World  ')",
                    Print(new StringTrim {Side = Constant(TrimSide.Start), String = Constant("  Hello World  ")}),
                    "Hello World  ");
                yield return new StepTestCase("Print StringTrim(Side: End, String: '  Hello World  ')",
                    Print(new StringTrim {Side = Constant(TrimSide.End), String = Constant("  Hello World  ")}),
                    "  Hello World");
                yield return new StepTestCase("Print StringTrim(Side: Both, String: '  Hello World  ')",
                    Print(new StringTrim {Side = Constant(TrimSide.Both), String = Constant("  Hello World  ")}),
                    HelloWorldString);


                yield return new StepTestCase("Print ValueIf(Condition: True, Else: 'World', Then: 'Hello')",
                    Print(new ValueIf<StringStream>
                    {
                        Condition = Constant(true),
                        Then = Constant("Hello"),
                        Else = Constant("World")
                    }), "Hello");


                yield return new StepTestCase("Print ValueIf(Condition: False, Else: 'World', Then: 'Hello')",
                    Print(new ValueIf<StringStream>
                    {
                        Condition = Constant(false),
                        Then = Constant("Hello"),
                        Else = Constant("World")
                    }), "World");

                yield return new StepTestCase("Print Match ArraySort(Array: ['B'; 'C'; 'A'], Descending: False)",
                    Print(new StringJoin
                    {
                        Delimiter = Constant(", "),
                        Strings = new ArraySort<StringStream>
                        {
                            Array = Array(("B"), ("C"), ("A")),
                            Descending = Constant(false)
                        }
                    }), "A, B, C");

                yield return new StepTestCase("Print Match ArraySort(Array: ['B'; 'C'; 'A'], Descending: True)",
                    Print(new StringJoin
                    {
                        Delimiter = Constant(", "),
                        Strings = new ArraySort<StringStream>
                        {
                            Array = Array(("B"), ("C"), ("A")),
                            Descending = Constant(true)
                        }
                    }), "C, B, A");

                yield return new StepTestCase("Print First index of ''World'' in ''Hello World, Goodbye World''",
                    Print(new FindSubstring
                    {
                        String = Constant("Hello World, Goodbye World"),
                        SubString = Constant("World")
                    }),
                    "6"
                );

                yield return new StepTestCase("Print Last index of ''World'' in ''Hello World, Goodbye World''",
                    Print(new FindLastSubstring
                    {
                        String = Constant("Hello World, Goodbye World"),
                        SubString = Constant("World")
                    }),
                    "21"
                );

                yield return new StepTestCase("Print Get character at index '1' in ''Hello World''",
                    Print(new CharAtIndex
                    {
                        Index = Constant(1),
                        String = Constant(HelloWorldString)

                    }), "e");

                yield return new StepTestCase("Repeat 'Print 'Hello World'' '3' times.",
                    new DoXTimes
                    {
                        X = Constant(3),
                        Action = Print(Constant(HelloWorldString))
                    }, HelloWorldString, HelloWorldString, HelloWorldString);


                yield return new StepTestCase("<Foo> = 'Hello'; Append ' World' to <Foo>; Print <Foo>",
                    Sequence(
                        SetVariable(FooVariableName, Constant("Hello")),
                        new AppendString {Variable = FooVariableName, String = Constant(" World")},
                        Print(GetVariable<StringStream>(FooVariableName))
                    ), HelloWorldString);

                yield return new StepTestCase("Print GetSubstring(Index: 6, Length: 2, String: 'Hello World')",
                    Print(new GetSubstring
                    {
                        String = Constant(HelloWorldString),
                        Index = Constant(6),
                        Length = Constant(2)
                    }), "Wo");

                yield return new StepTestCase("Print ElementAtIndex(Array: ['Hello'; 'World'], Index: 1)",
                    Print(new ElementAtIndex<StringStream>
                    {
                        Array = Array(("Hello"), ("World")),
                        Index = Constant(1)
                    }), "World"
                );


                yield return new StepTestCase("Print 'I have more config'", new Print<StringStream>
                {
                    Value = Constant("I have more config"),
                    Configuration = new Configuration
                    {
                        Priority = 1,
                        TargetMachineTags = new List<string>
                        {
                            "Tag1",
                            "Tag2"
                        },
                        DoNotSplit = true,
                        AdditionalRequirements = new List<Requirement>
                        {
                            new Requirement
                            {
                                MinVersion = new Version(1, 0),
                                MaxVersion = new Version(2, 0),
                                Name = "ValueIf",
                                Notes = "ABC123"
                            }
                        }
                    }
                }, "I have more config");

                yield return new StepTestCase("AssertTrue(Boolean: True)", new AssertTrue
                {
                    Boolean = Constant(true)
                });

                yield return new StepTestCase("AssertError(Step: AssertTrue(Boolean: False))", new AssertError
                {
                    Step = new AssertTrue
                    {
                        Boolean = Constant(false)
                    }
                });

                yield return new StepTestCase("Read CSV",
                    new EntityForEach
                    {

                        EntityStream = new FromCSV
                        {
                            Delimiter = Constant(","),
                            Stream = Constant(@"Name,Summary
One,The first number
Two,The second number"),
                        },
                        Action = new Print<Record>{Value = new GetVariable<Record> {Variable = VariableName.Entity}}
                    },
                    "Name: One, Summary: The first number",
                    "Name: Two, Summary: The second number"
                );



                yield return new StepTestCase("Foreach nested array",
                    Sequence(new IStep<Unit>[]
                    {
                        new SetVariable<List<List<StringStream>>>
                        {
                            Variable = new VariableName("DataVar"),

                            Value = new Array<List<StringStream>>
                            {
                                Elements = new[]
                                {
                                    new Array<StringStream>
                                    {
                                        Elements = new[] {Constant("One"), Constant("The first number")}
                                    },
                                    new Array<StringStream>
                                    {
                                        Elements = new[] {Constant("Two"), Constant("The second number")}
                                    },

                                }
                            }
                        },

                        new ForEach<List<string>>
                        {
                            Array = GetVariable<List<List<string>>>(new VariableName("DataVar")),
                            Variable = FooVariableName,
                            Action = new Print<StringStream>
                            {
                                Value = new ElementAtIndex<StringStream>
                                {
                                    Array = new GetVariable<List<StringStream>> {Variable = FooVariableName},
                                    Index = new IntConstant(0)
                                }
                            },
                            Configuration = new Configuration
                            {
                                TargetMachineTags = new List<string> {"Tag1"}
                            }
                        }

                    }), "One", "Two"

                );
                //{IgnoreName = true};


            }
        }

        private static GetVariable<T> GetVariable<T>(VariableName variableName) => new GetVariable<T>{Variable = variableName};


        //private static Constant<StringStream> Constant(string s) => new StringConstant(new StringStream(s));

        //private static Constant<T> Constant<T>(T element) => new Constant<T>(element);

        private static Print<T> Print<T>(IStep<T> element) => new Print<T>{Value = element};

        //private static Array<T> Array<T>(params IStep<T>[] elements)=> new Array<T>{Elements = elements};
        private static SetVariable<T> SetVariable<T>(VariableName variableName, IStep<T> step) =>
            new SetVariable<T>
            {
                Variable = variableName,
                Value = step
            };

        private static Sequence<Unit> Sequence(params IStep<Unit>[] steps)=> new Sequence<Unit>{InitialSteps = steps.SkipLast(1).ToList(), FinalStep = steps.Last()};


        private class StepTestCase : ITestBaseCaseParallel
        {
            public StepTestCase(string expectedName, IStep step, params string[] expectedLoggedValues)
            {
                Step = step;
                ExpectedLoggedValues = expectedLoggedValues;
                ExpectedName = expectedName;
            }



            public string ExpectedName { get; }

            /// <inheritdoc />
            public string Name => ExpectedName + (AddConfiguration? "With Config" : "");

            public IStep Step { get; }

            /// <summary>
            /// If true, adds configuration to the top step.
            /// </summary>
            public bool AddConfiguration {get; set; }


            public IReadOnlyList<string> ExpectedLoggedValues { get; }

            public bool IgnoreLoggedValues { get; set; }
            //public bool IgnoreName { get; set; }

            /// <inheritdoc />
            public async Task ExecuteAsync(ITestOutputHelper outputHelper)
            {
                //Arrange
                var pfs = StepFactoryStore.CreateUsingReflection(typeof(StepFactory));
                var logger = new TestLogger();
                var yamlRunner = new SequenceRunner(EmptySettings.Instance, logger, ExternalProcessRunner.Instance, FileSystemHelper.Instance, pfs);

                //Act

                if (AddConfiguration)
                    Step.Configuration = new Configuration
                    {
                        TargetMachineTags = new List<string>
                        {
                            "ValueIf Tag"
                        }
                    };

                var text = await Step.SerializeAsync(CancellationToken.None);

                outputHelper.WriteLine(text);
                var runResult = await yamlRunner.RunSequenceFromTextAsync(text, CancellationToken.None);

                //Assert
                runResult.ShouldBeSuccessful(x=>x.AsString);

                if(!IgnoreLoggedValues)
                    logger.LoggedValues.Should().BeEquivalentTo(ExpectedLoggedValues);
                //if(!IgnoreName)
                //    Step.Name.Should().Be(ExpectedName);

            }

        }
    }
}