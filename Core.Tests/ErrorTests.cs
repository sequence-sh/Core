using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests
{

    public class RunErrorTests : RunErrorTestCases
    {
        public RunErrorTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory]

        [ClassData(typeof(RunErrorTestCases))]
        public override Task Test(string key) => base.Test(key);
    }

    public class RunErrorTestCases : TestBaseParallel
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCaseParallel> TestCases
        {
            get
            {
                yield return new ErrorTestFunction("Get Missing Variable",
                    new GetVariable<string>
                    {
                        VariableName = FooString
                    },
                    new SingleError("Variable '<Foo>' does not exist.", "<Foo>", null, ErrorCode.MissingVariable));

                yield return new ErrorTestFunction("Test assert",
                    new AssertTrue
                    {
                        Test = new Constant<bool>(false)
                    }, new SingleError($"Assertion Failed '{false}'", "AssertTrue(Test: False)", null, ErrorCode.IndexOutOfBounds));


                yield return new ErrorTestFunction("Get variable with wrong type",
                    new Sequence
                    {
                        Steps = new IStep<Unit>[]
                        {
                            new SetVariable<int>
                            {
                                VariableName = FooString,
                                Value = new Constant<int>(42)
                            },

                            new Print<bool>
                            {
                                Value = new GetVariable<bool>
                                {
                                    VariableName =FooString
                                }
                            }
                        }
                    },
                    new SingleError("Variable '<Foo>' does not have type 'System.Boolean'.", "<Foo>", null, ErrorCode.WrongVariableType)
                );

                yield return new ErrorTestFunction("Assert Error with succeeding step",
                    new AssertError
                    {
                        Test = new AssertTrue
                        {
                            Test = new Constant<bool>(true)
                        }
                    }, new SingleError("Expected an error but step was successful.", "AssertError(Test: AssertTrue(Test: True))", null, ErrorCode.AssertionFailed));


                yield return new ErrorTestFunction("Divide by zero",
                    new ApplyMathOperator
                    {
                        Left = new Constant<int>(1),
                        Right = new Constant<int>(0),
                        Operator = new Constant<MathOperator>(MathOperator.Divide)
                    },
                    new SingleError("Divide by Zero Error", "ApplyMathOperator(Left: 1, Operator: /, Right: 0)", null, ErrorCode.DivideByZero));

                yield return new ErrorTestFunction("Array Index minus one",
                    new ElementAtIndex<bool>
                    {
                        Array = new Array<bool>(){Elements = new []{new Constant<bool>(true)}},
                        Index = new Constant<int>(-1)
                    },
                    new SingleError("Index was out of the range of the array.", "ElementAtIndex(Array: [True], Index: -1)", null, ErrorCode.IndexOutOfBounds)
                    );

                yield return new ErrorTestFunction("Array Index out of bounds",
                    new ElementAtIndex<bool>
                    {
                        Array = new Array<bool>
                        {
                            Elements = new []{new Constant<bool>(true), new Constant<bool>(false)}
                        },
                        Index = new Constant<int>(5)
                    },
                    new SingleError("Index was out of the range of the array.", "ElementAtIndex(Array: [True; False], Index: 5)", null, ErrorCode.IndexOutOfBounds)
                    );


                yield return new ErrorTestFunction("Get letter minus one",
                    new GetLetterAtIndex
                    {
                        Index = new Constant<int>(-1),
                        String = new Constant<string>("Foo")
                    }, new SingleError("Index was outside the bounds of the string", "Get character at index '-1' in ''Foo''", null, ErrorCode.IndexOutOfBounds));

                yield return new ErrorTestFunction("Get letter out of bounds",
                    new GetLetterAtIndex
                    {
                        Index = new Constant<int>(5),
                        String = new Constant<string>("Foo")
                    }, new SingleError("Index was outside the bounds of the string", "Get character at index '5' in ''Foo''", null, ErrorCode.IndexOutOfBounds));

                yield return new ErrorTestFunction("Get substring minus one",
                    new GetSubstring
                    {
                        Index = new Constant<int>(-1),
                        String = new Constant<string>("Foo"),
                        Length = new Constant<int>(10)
                    }, new SingleError("Index was outside the bounds of the string", "GetSubstring(Index: -1, Length: 10, String: 'Foo')", null, ErrorCode.IndexOutOfBounds));

                yield return new ErrorTestFunction("Get substring out of bounds",
                    new GetSubstring
                    {
                        Index = new Constant<int>(5),
                        String = new Constant<string>("Foo"),
                        Length = new Constant<int>(10)
                    }, new SingleError("Index was outside the bounds of the string", "GetSubstring(Index: 5, Length: 10, String: 'Foo')", null, ErrorCode.IndexOutOfBounds));



            }
        }


        public static readonly VariableName FooString = new VariableName("Foo");

        private class ErrorTestFunction : ITestBaseCaseParallel
        {
            public ErrorTestFunction(string name, IStep process, IError expectedErrors)
            {
                Name = name;
                Process = process;
                ExpectedErrors = expectedErrors;
            }

            /// <inheritdoc />
            public string Name { get; }


            public IStep Process { get; }

            public IError ExpectedErrors { get; }


            /// <inheritdoc />
            public async Task ExecuteAsync(ITestOutputHelper testOutputHelper)
            {
                var spf = StepFactoryStore.CreateUsingReflection(typeof(IStep));

                var state = new StateMonad(NullLogger.Instance, EmptySettings.Instance, ExternalProcessRunner.Instance, spf);

                var r = await Process.Run<object>(state, CancellationToken.None);

                r.IsFailure.Should().BeTrue("Step should have failed");

                r.Error.GetAllErrors().Should().BeEquivalentTo(ExpectedErrors.GetAllErrors());

            }
        }
    }
}