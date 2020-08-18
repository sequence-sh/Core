using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Tests.Extensions;
using Xunit;

namespace Reductech.EDR.Processes.Tests.NewProcessesTests
{
    public class ProcessTests : TestBase
    {
        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(ProcessTests))]
        public override void Test(string key) => base.Test(key);

        public const string HelloWorldString = "Hello World";
        public static readonly VariableName FooString = new VariableName("Foo");
        public static readonly VariableName BarString = new VariableName("Bar");

        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases
        {
            get
            {
                yield return new TestCase("Print 'Hello World'", Print (Constant(HelloWorldString)), HelloWorldString);

                yield return new TestCase("Foo = Hello World; Print '<Foo>'",Sequence
                    (
                        SetVariable(FooString, Constant(HelloWorldString)),
                        Print(new GetVariable<string>(FooString))), HelloWorldString);

                yield return new TestCase("Foo = Hello World; Bar = <Foo>; Print '<Bar>'", Sequence(SetVariable(FooString, Constant(HelloWorldString)),
                        SetVariable(BarString, new GetVariable<string>(FooString)),
                        Print(new GetVariable<string>(BarString))), HelloWorldString);


                yield return new TestCase("Foo = 1 LessThan 2; Print '<Foo>'",Sequence(
                    SetVariable(FooString, new Compare<int>
                    {
                        Left = Constant(1),
                        Operator = Constant(CompareOperator.LessThan),
                        Right = Constant(2)
                    }),
                    Print(new GetVariable<bool>(FooString))

                    ), true.ToString());


                yield return new TestCase("Print 'True And Not False'",
                    Print(new ApplyBooleanOperator
                    {
                        Left = Constant(true),
                        Right = new Not { Boolean = Constant(false) },
                        Operator = Constant(BooleanOperator.And)
                    }), true.ToString());

                yield return new TestCase("Print 'False Or Not False'",
                    Print(new ApplyBooleanOperator
                    {
                        Left = Constant(false),
                        Right = new Not { Boolean = Constant(false) },
                        Operator = Constant(BooleanOperator.Or)
                    }), true.ToString());

                yield return new TestCase("Foreach Foo in [Hello; World]; Print '<Foo>'",
                    new ForEach<string>
                    {
                        Action = Print(new GetVariable<string>(FooString)),
                        Array = Array(Constant("Hello"),
                            Constant("World")),
                        VariableName = FooString
                    }, "Hello", "World");

                yield return new TestCase("If True then Print 'Hello World' else Print 'World Hello'",
                    new Conditional
                    {
                        Condition = Constant(true),
                        ThenProcess = Print(Constant(HelloWorldString)),
                        ElseProcess = Print(Constant("World Hello"))
                    },
                    HelloWorldString);


                yield return new TestCase("For Foo = 5; Foo <= 10; += 2; Print '<Foo>'",
                    new For
                    {
                        VariableName = FooString,
                        Action = Print(new GetVariable<int>(FooString)),
                        From = Constant(5),
                        To = Constant(10),
                        Increment = Constant(2)
                    },
                    "5", "7", "9");

                yield return new TestCase("Foo = True; Repeat 'Print 'Hello World'; Foo = False' while '<Foo>'",
                    Sequence(SetVariable(FooString, Constant(true)),
                            new RepeatWhile
                            {
                                Action = Sequence(Print(Constant(HelloWorldString)),
                                        SetVariable(FooString, Constant(false))),
                                Condition = new GetVariable<bool>(FooString)
                            }),
                    HelloWorldString);

                yield return new TestCase("Print 'ApplyMathOperator(Left: 2, Operator: Times, Right: 3)'",
                    Print(new ApplyMathOperator
                    {
                        Left = Constant(2),
                        Right = Constant(3),
                        Operator = Constant(MathOperator.Times)
                    }), "6");

                yield return new TestCase("Print 'ArrayCount(Array: [Hello; World])'",
                    Print(new ArrayCount<string>
                    {
                        Array = Array(Constant("Hello"),
                                    Constant("World"))
                    }),
                    "2"
                );

                yield return new TestCase("Print 'ArrayIsEmpty(Array: [])'",
                    Print(new ArrayIsEmpty<string>{Array = Array<string>()}), true.ToString());

                yield return new TestCase("Print 'ArrayIsEmpty(Array: [Hello World])'",
                    Print(new ArrayIsEmpty<string>
                    {
                        Array = Array(Constant(HelloWorldString))
                    }), false.ToString());

                yield return new TestCase("Print 'Length of Hello World'",
                    Print(new LengthOfString
                    {
                        String = Constant(HelloWorldString)
                    }), "11");

                yield return new TestCase("Print ''' is empty?'",
                    Print(new StringIsEmpty
                    {
                        String = Constant("")
                    }),true.ToString()
                    );

                yield return new TestCase("Print ''Hello World' is empty?'",
                    Print(new StringIsEmpty
                    {
                        String = Constant(HelloWorldString)
                    }), false.ToString()
                    );

                yield return new TestCase("Print 'FirstIndexOfElement(Array: [Hello; World], Element: World)'",
                    Print(new FirstIndexOfElement<string>
                    {
                        Array = Array(Constant("Hello"), Constant("World")),
                        Element = Constant("World")
                    }),
                    1.ToString()
                    );

                yield return new TestCase("Print 'FirstIndexOfElement(Array: [Hello; World], Element: Goodbye)'",
                    Print(new FirstIndexOfElement<string>
                    {
                        Array = Array(Constant("Hello"), Constant("World")),
                        Element = Constant("Goodbye")
                    }),
                    (-1).ToString()
                    );

                yield return new TestCase("Print 'Join Repeat(Element: Hello, Number: 3)'",Print(new JoinStrings
                {
                    Delimiter = Constant(", "),
                    List = new Repeat<string>
                    {
                        Number = Constant(3),
                        Element = Constant("Hello")
                    }
                }), "Hello, Hello, Hello");

                yield return new TestCase("Print 'ElementAtIndex(Array: SplitString(String: Hello, World, Delimiter: , ), Index: 1)'",
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

                yield return new TestCase("Foo = 2; IncrementVariable(Variable: Foo, Amount: 3); Print '<Foo>'",
                    Sequence(SetVariable(FooString, Constant(2)),
                    new IncrementVariable
                    {
                        Amount = Constant(3),
                        Variable = FooString
                    },
                    Print(new GetVariable<int>(FooString))
                    ),

                    5.ToString());

                yield return new TestCase("Print 'ToCase(String: Hello World, Case: Upper)'", Print(new ToCase{Case = Constant(TextCase.Upper), String = Constant(HelloWorldString)}),"HELLO WORLD");
                yield return new TestCase("Print 'ToCase(String: Hello World, Case: Lower)'", Print(new ToCase{Case = Constant(TextCase.Lower), String = Constant(HelloWorldString)}),"hello world");
                yield return new TestCase("Print 'ToCase(String: Hello World, Case: Title)'", Print(new ToCase{Case = Constant(TextCase.Title), String = Constant(HelloWorldString)}),"Hello World");


                yield return new TestCase("Print 'Trim(String:   Hello World  , Side: Left)'", Print(new Trim {Side = Constant(TrimSide.Left), String = Constant("  Hello World  ")}), "Hello World  ");
                yield return new TestCase("Print 'Trim(String:   Hello World  , Side: Right)'", Print(new Trim {Side = Constant(TrimSide.Right), String = Constant("  Hello World  ")}), "  Hello World");
                yield return new TestCase("Print 'Trim(String:   Hello World  , Side: Both)'", Print(new Trim {Side = Constant(TrimSide.Both), String = Constant("  Hello World  ")}), HelloWorldString);


                yield return new TestCase("Print 'Test(Condition: True, ThenValue: Hello, ElseValue: World)'", Print(new Test<string>
                {
                    Condition = Constant(true),
                    ThenValue = Constant("Hello"),
                    ElseValue = Constant("World")
                }), "Hello");


                yield return new TestCase("Print 'Test(Condition: False, ThenValue: Hello, ElseValue: World)'", Print(new Test<string>
                {
                    Condition = Constant(false),
                    ThenValue = Constant("Hello"),
                    ElseValue = Constant("World")
                }), "World");

                yield return new TestCase("Print 'Join SortArray(Array: [B; C; A], Order: Ascending)'",
                    Print(new JoinStrings
                    {
                        Delimiter = Constant(", "),
                        List = new SortArray<string>
                        {
                            Array = Array(Constant("B"), Constant("C"), Constant("A")),
                            Order = Constant(SortOrder.Ascending)
                        }
                    }), "A, B, C");

                yield return new TestCase("Print 'Join SortArray(Array: [B; C; A], Order: Descending)'",
                    Print(new JoinStrings
                    {
                        Delimiter = Constant(", "),
                        List = new SortArray<string>
                        {
                            Array = Array(Constant("B"), Constant("C"), Constant("A")),
                            Order = Constant(SortOrder.Descending)
                        }
                    }), "C, B, A");

                yield return new TestCase("Print 'First index of 'World' in 'Hello World, Goodbye World''",
                    Print(new FirstIndexOf
                    {
                        String = Constant("Hello World, Goodbye World"),
                        SubString = Constant("World" )
                    }),
                    "6"
                    );

                yield return new TestCase("Print 'Last index of 'World' in 'Hello World, Goodbye World''",
                    Print(new LastIndexOf
                    {
                        String = Constant("Hello World, Goodbye World"),
                        SubString = Constant("World")
                    }),
                    "21"
                    );

                yield return new TestCase("Print 'Get character at index '1' in 'Hello World''",
                    Print(new GetLetterAtIndex
                    {
                        Index = Constant(1),
                        String = Constant(HelloWorldString)

                    }), "e");

                yield return new TestCase("Repeat 'Print 'Hello World'' '3' times.",
                    new RepeatXTimes
                    {
                        Number = Constant(3),
                        Action = Print(Constant(HelloWorldString))
                    },HelloWorldString, HelloWorldString, HelloWorldString);


                yield return new TestCase("Foo = Hello; Append  World to Foo; Print '<Foo>'",
                    Sequence(
                        SetVariable(FooString,Constant("Hello")),
                        new AppendString{Variable = FooString, String = Constant(" World") },
                        Print(new GetVariable<string>(FooString))
                        ), HelloWorldString);

                yield return new TestCase("Print 'GetSubstring(String: Hello World, Index: 6, Length: 2)'",
                    Print(new GetSubstring
                    {
                        String = Constant(HelloWorldString),
                        Index = Constant(6),
                        Length = Constant(2)
                    }), "Wo");

                yield return new TestCase("Print 'ElementAtIndex(Array: [Hello; World], Index: 1)'",
                    Print(new ElementAtIndex<string>
                    {
                        Array = Array(Constant("Hello"), Constant("World")),
                        Index = Constant(1)
                    }), "World"
                    );
            }
        }

        private static Constant<T> Constant<T>(T element) => new Constant<T>(element);

        private static Print<T> Print<T>(IRunnableProcess<T> element) => new Print<T>{Value = element};

        private static Array<T> Array<T>(params IRunnableProcess<T>[] elements)=> new Array<T>{Elements = elements};
        private static SetVariable<T> SetVariable<T>(VariableName variableName, IRunnableProcess<T> runnableProcess) => new SetVariable<T>(variableName, runnableProcess);

        private static Sequence Sequence(params IRunnableProcess<Unit>[] steps)=> new Sequence{Steps = steps};


        private class TestCase : ITestCase
        {
            public TestCase(string expectedName, IRunnableProcess runnableProcess, params string[] expectedLoggedValues)
            {
                RunnableProcess = runnableProcess;
                ExpectedLoggedValues = expectedLoggedValues;
                ExpectedName = expectedName;
            }

            public string ExpectedName { get; }

            /// <inheritdoc />
            public string Name => ExpectedName;

            public IRunnableProcess RunnableProcess { get; }


            public IReadOnlyList<string> ExpectedLoggedValues { get; }

            /// <inheritdoc />
            public void Execute()
            {
                RunnableProcess.Name.Should().Be(ExpectedName);

                var unfrozen = RunnableProcess.Unfreeze();

                var yaml = unfrozen.SerializeToYaml();

                var pfs = ProcessFactoryStore.CreateUsingReflection(typeof(RunnableProcessFactory));
                var logger = new TestLogger();

                var yamlRunner = new YamlRunner(EmptySettings.Instance, logger, pfs);

                var runResult = yamlRunner.RunProcessFromYamlString(yaml);

                runResult.ShouldBeSuccessful();

                logger.LoggedValues.Should().BeEquivalentTo(ExpectedLoggedValues);

            }
        }
    }

    public class TestLogger : ILogger
    {
        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (state is FormattedLogValues flv)
                foreach (var formattedLogValue in flv)
                    LoggedValues.Add(formattedLogValue.Value);
            else throw new NotImplementedException();
        }

        public List<object> LoggedValues = new List<object>();

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }

}
