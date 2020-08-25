using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Test.Extensions;
using Xunit;
using Xunit.Abstractions;
using ITestCase = Reductech.EDR.Processes.Test.Extensions.ITestCase;

namespace Reductech.EDR.Processes.Test
{

    public class ErrorTests : ErrorTestCases
    {
        public ErrorTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(ErrorTestCases))]
        public override void Test(string key) => base.Test(key);
    }

    public class ErrorTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases {
            get
            {
                yield return new ErrorTestCase("Get Missing Variable",
                    new GetVariable<string>
                    {
                        VariableName = FooString
                    },
                    new RunError($"Variable 'Foo' does not exist.", "<Foo>", null, ErrorCode.MissingVariable));

                yield return new ErrorTestCase("Test assert",
                    new AssertTrue
                    {
                        Test = new Constant<bool>(false)
                    }, new RunError($"Assertion Failed '{false}'", "AssertTrue(Test: False)", null, ErrorCode.IndexOutOfBounds));


                yield return new ErrorTestCase("Get variable with wrong type",
                    new Sequence
                    {
                        Steps = new IRunnableProcess<Unit>[]
                        {
                            new SetVariable<int>
                            {
                                VariableName = FooString,
                                Value = new Constant<int>(42)
                            },

                            new Print<bool>
                            {
                                Value = new GetVariable<bool>()
                            {
                                VariableName =FooString
                            }
                            }
                        }
                    },
                     new RunError("Variable 'Foo' does not have type 'System.Boolean'.", "<Foo>", null, ErrorCode.WrongVariableType)
                    );


            }
        }


        public static readonly VariableName FooString = new VariableName("Foo");

        public class ErrorTestCase : ITestCase
        {
            public ErrorTestCase(string name, IRunnableProcess process, IRunErrors expectedErrors)
            {
                Name = name;
                Process = process;
                ExpectedErrors = expectedErrors;
            }

            /// <inheritdoc />
            public string Name { get; }


            public IRunnableProcess Process { get; }

            public IRunErrors ExpectedErrors { get; }


            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var state = new ProcessState(NullLogger.Instance, EmptySettings.Instance);

                var r = Process.Run<object>(state);

                r.IsFailure.Should().BeTrue("Process should have failed");

                r.Error.AllErrors.Should().BeEquivalentTo(ExpectedErrors.AllErrors);

            }
        }
    }
}