using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ForTests : StepTestBase<For, Unit>
    {
        /// <inheritdoc />
        public ForTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Increment 1",
                    new For
                    {
                        Action = new Print<int> {Value = GetVariable<int>("Foo")},
                        From = Constant(1),
                        To = Constant(3),
                        Increment = Constant(1),
                        VariableName = new VariableName("Foo")
                    }, Unit.Default,"1","2","3").WithExpectedFinalState("Foo", 4);

                yield return new StepCase("Increment 2",
                    new For
                    {
                        Action = new Print<int> { Value = GetVariable<int>("Foo") },
                        From = Constant(1),
                        To = Constant(6),
                        Increment = Constant(2),
                        VariableName = new VariableName("Foo")
                    }, Unit.Default, "1","3","5")
                    .WithExpectedFinalState("Foo", 7);

                yield return new StepCase("Increment -1",
                    new For
                    {
                        Action = new Print<int> { Value = GetVariable<int>("Foo") },
                        From = Constant(3),
                        To = Constant(1),
                        Increment = Constant(-1),
                        VariableName = new VariableName("Foo")
                    }, Unit.Default, "3","2","1"
                    ).WithExpectedFinalState("Foo", 0);

                yield return new StepCase("Increment No range",
                    new For
                    {
                        Action = new Print<int> { Value = GetVariable<int>("Foo") },
                        From = Constant(3),
                        To = Constant(1),
                        Increment = Constant(1),
                        VariableName = new VariableName("Foo")
                    }, Unit.Default
                    ).WithExpectedFinalState("Foo", 3);


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Increment 1",
                    "For(Action = Print(Value = <Foo>), From = 1, To = 3, Increment = 1, VariableName = <Foo>)",
                    Unit.Default,
                    "1","2","3"
                    ).WithExpectedFinalState("Foo", 4);
            }

        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                yield return new ErrorCase("Test increment 0",
                    new For
                    {
                        Action = new Print<int> { Value = GetVariable<int>("Foo") },
                        From = Constant(1),
                        To = Constant(3),
                        Increment = Constant(0),
                        VariableName = new VariableName("Foo")
                    },
                    new ErrorBuilder("Cannot do a For loop with an increment of 0", ErrorCode.DivideByZero))
                        .WithExpectedFinalState("Foo", 1)
                    ;

                yield return CreateDefaultErrorCase();
            }
        }
    }
}