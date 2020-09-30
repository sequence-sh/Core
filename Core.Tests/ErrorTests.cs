using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Internal;
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
        public override void Test(string key) => base.Test(key);
    }

    public class RunErrorTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCase> TestCases
        {
            get
            {
                yield return new ErrorTestFunction("Get Missing Variable",
                    new GetVariable<string>
                    {
                        VariableName = FooString
                    },
                    new RunError("Variable '<Foo>' does not exist.", "<Foo>", null, ErrorCode.MissingVariable));

                yield return new ErrorTestFunction("Test assert",
                    new AssertTrue
                    {
                        Test = new Constant<bool>(false)
                    }, new RunError($"Assertion Failed '{false}'", "AssertTrue(Test: False)", null, ErrorCode.IndexOutOfBounds));


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
                    new RunError("Variable '<Foo>' does not have type 'System.Boolean'.", "<Foo>", null, ErrorCode.WrongVariableType)
                );

                yield return new ErrorTestFunction("Assert Error with succeeding step",
                    new AssertError
                    {
                        Test = new AssertTrue
                        {
                            Test = new Constant<bool>(true)
                        }
                    }, new RunError("Expected an error but step was successful.", "AssertError(Test: AssertTrue(Test: True))", null, ErrorCode.AssertionFailed));


            }
        }


        public static readonly VariableName FooString = new VariableName("Foo");

        private class ErrorTestFunction : ITestBaseCase
        {
            public ErrorTestFunction(string name, IStep process, IRunErrors expectedErrors)
            {
                Name = name;
                Process = process;
                ExpectedErrors = expectedErrors;
            }

            /// <inheritdoc />
            public string Name { get; }


            public IStep Process { get; }

            public IRunErrors ExpectedErrors { get; }


            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var state = new StateMonad(NullLogger.Instance, EmptySettings.Instance, ExternalProcessRunner.Instance);

                var r = Process.Run<object>(state);

                r.IsFailure.Should().BeTrue("Step should have failed");

                r.Error.AllErrors.Should().BeEquivalentTo(ExpectedErrors.AllErrors);

            }
        }
    }
}