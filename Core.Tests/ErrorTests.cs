using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

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
                    new GetVariable<StringStream>
                    {
                        Variable = FooString
                    },
                    new ErrorBuilder("Variable '<Foo>' does not exist.", ErrorCode.MissingVariable));

                yield return new ErrorTestFunction("ValueIf assert",
                    new AssertTrue
                    {
                        Boolean = new BoolConstant(false)
                    }, new ErrorBuilder($"Assertion Failed '{false}'", ErrorCode.IndexOutOfBounds));


                yield return new ErrorTestFunction("Get variable with wrong type",
                    new Core.Steps.Sequence<Unit>
                    {
                        InitialSteps = new IStep<Unit>[]
                        {
                            new SetVariable<int>
                            {
                                Variable = FooString,
                                Value = new IntConstant(42)
                            },

                            new Print<bool>
                            {
                                Value = new GetVariable<bool>
                                {
                                    Variable =FooString
                                }
                            }
                        },
                        FinalStep = new DoNothing()
                    },
                    new ErrorBuilder("Variable '<Foo>' does not have type 'System.Boolean'.", ErrorCode.WrongVariableType)
                        .WithLocation(new GetVariable<bool>
                        {
                            Variable = FooString
                        })
                );

                yield return new ErrorTestFunction("Assert Error with succeeding step",
                    new AssertError
                    {
                        Step = new AssertTrue
                        {
                            Boolean = new BoolConstant(true)
                        }
                    }, new ErrorBuilder("Expected an error but step was successful.", ErrorCode.AssertionFailed));


                yield return new ErrorTestFunction("Divide by zero",
                    new ApplyMathOperator
                    {
                        Left = new IntConstant(1),
                        Right = new IntConstant(0),
                        Operator = Constant(MathOperator.Divide)
                    },
                    new ErrorBuilder("Divide by Zero Error", ErrorCode.DivideByZero));

                yield return new ErrorTestFunction("Array Index minus one",
                    new ElementAtIndex<bool>
                    {
                        Array = Array(true) ,
                        Index = new IntConstant(-1)
                    },
                    new ErrorBuilder("Index was less than zero.", ErrorCode.IndexOutOfBounds)
                    );

                yield return new ErrorTestFunction("Array Index out of bounds",
                    new ElementAtIndex<bool>
                    {
                        Array = new ArrayNew<bool>
                        {
                            Elements = new []{new BoolConstant(true), new BoolConstant(false)}
                        },
                        Index = new IntConstant(5)
                    },
                    new ErrorBuilder("Index was out of the range of the array.", ErrorCode.IndexOutOfBounds)
                    );


                yield return new ErrorTestFunction("Get letter minus one",
                    new CharAtIndex
                    {
                        Index = new IntConstant(-1),
                        String = Constant("Foo")
                    }, new ErrorBuilder("Index was outside the bounds of the string", ErrorCode.IndexOutOfBounds));

                yield return new ErrorTestFunction("Get letter out of bounds",
                    new CharAtIndex
                    {
                        Index = new IntConstant(5),
                        String = Constant("Foo")
                    }, new ErrorBuilder("Index was outside the bounds of the string", ErrorCode.IndexOutOfBounds));

                yield return new ErrorTestFunction("Get substring minus one",
                    new GetSubstring
                    {
                        Index = new IntConstant(-1),
                        String = Constant("Foo"),
                        Length = new IntConstant(10)
                    }, new ErrorBuilder("Index was outside the bounds of the string", ErrorCode.IndexOutOfBounds));

                yield return new ErrorTestFunction("Get substring out of bounds",
                    new GetSubstring
                    {
                        Index = new IntConstant(5),
                        String = Constant("Foo"),
                        Length = new IntConstant(10)
                    }, new ErrorBuilder("Index was outside the bounds of the string", ErrorCode.IndexOutOfBounds));



            }
        }


        public static readonly VariableName FooString = new VariableName("Foo");

        private class ErrorTestFunction : ITestBaseCaseParallel
        {
            public ErrorTestFunction(string name, IStep process, IErrorBuilder expectedErrors)
            {
                Name = name;
                Process = process;
                ExpectedErrors = expectedErrors.WithLocation(process);
            }

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

                using var state = new StateMonad(NullLogger.Instance, EmptySettings.Instance, ExternalProcessRunner.Instance, FileSystemHelper.Instance, spf);

                var r = await Process.Run<object>(state, CancellationToken.None);

                r.IsFailure.Should().BeTrue("Step should have failed");

                r.Error.GetAllErrors().Should().BeEquivalentTo(ExpectedErrors.GetAllErrors());

            }
        }
    }
}