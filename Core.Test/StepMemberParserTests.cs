using System.Collections.Generic;
using FluentAssertions;
using Reductech.EDR.Core.General;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Test.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Test
{
    public class StepMemberParserTests : StepMemberParserTestCases
    {
        public StepMemberParserTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(StepMemberParserTestCases))]
        public override void Test(string key) => base.Test(key);
    }

    public class StepMemberParserTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestFunction> TestCases {
            get
            {
                yield return new StepMemberTestFunction("true", new StepMember(new ConstantFreezableStep(true)));
                yield return new StepMemberTestFunction("True", new StepMember(new ConstantFreezableStep(true)));
                yield return new StepMemberTestFunction("1", new StepMember(new ConstantFreezableStep(1)));
                yield return new StepMemberTestFunction("\"Hello World\"", new StepMember(new ConstantFreezableStep("Hello World")));
                yield return new StepMemberTestFunction("Foo", null);
                yield return new StepMemberTestFunction("true true", null);

                yield return new StepMemberTestFunction("not (true)",
                    new StepMember(
                        new CompoundFreezableStep(NotStepFactory.Instance,
                            new FreezableStepData(new Dictionary<string, StepMember>()
                            {
                                {nameof(Not.Boolean), new StepMember(new ConstantFreezableStep(true))}
                            }), null
                        ))

                );

                yield return new StepMemberTestFunction("[true, false]", new StepMember(new List<IFreezableStep>()
                {
                    new ConstantFreezableStep(true),
                    new ConstantFreezableStep(false)
                }));

                yield return new StepMemberTestFunction("<foo> = 1", new StepMember(
                    new CompoundFreezableStep(SetVariableStepFactory.Instance,
                        new FreezableStepData(new Dictionary<string, StepMember>
                        {
                            {nameof(SetVariable<int>.VariableName), new StepMember( new VariableName("foo"))},
                            {nameof(SetVariable<int>.Value), new StepMember(new ConstantFreezableStep(1))}
                        }), null)));

                yield return new StepMemberTestFunction("1 + 2", new StepMember(
                    new CompoundFreezableStep(ApplyMathOperatorStepFactory.Instance,
                        new FreezableStepData(new Dictionary<string, StepMember>
                        {
                            {nameof(ApplyMathOperator.Left), new StepMember(new ConstantFreezableStep(1))},
                            {nameof(ApplyMathOperator.Operator), new StepMember(new ConstantFreezableStep(MathOperator.Add))},
                            {nameof(ApplyMathOperator.Right), new StepMember(new ConstantFreezableStep(2))}
                        }), null
                    )));

                yield return new StepMemberTestFunction("1 + <foo>", new StepMember(
                    new CompoundFreezableStep(ApplyMathOperatorStepFactory.Instance,
                        new FreezableStepData(new Dictionary<string, StepMember>
                        {
                            {nameof(ApplyMathOperator.Left), new StepMember(new ConstantFreezableStep(1))},
                            {nameof(ApplyMathOperator.Operator), new StepMember(new ConstantFreezableStep(MathOperator.Add))},
                            {nameof(ApplyMathOperator.Right), GetVariableStepMember("foo")}
                        }), null
                    )));

                yield return new StepMemberTestFunction("<foo> + 2", new StepMember(
                    new CompoundFreezableStep(ApplyMathOperatorStepFactory.Instance,
                        new FreezableStepData(new Dictionary<string, StepMember>
                        {
                            {nameof(ApplyMathOperator.Operator), new StepMember(new ConstantFreezableStep(MathOperator.Add))},
                            {nameof(ApplyMathOperator.Left), GetVariableStepMember("foo")},
                            {nameof(ApplyMathOperator.Right), new StepMember(new ConstantFreezableStep(2))}
                        }), null
                    )));

                yield return new StepMemberTestFunction("Print(Value = 1)", new StepMember(
                    new CompoundFreezableStep(PrintStepFactory.Instance,
                        new FreezableStepData(new Dictionary<string, StepMember>
                        {
                            {nameof(Print<int>.Value), new StepMember(new ConstantFreezableStep(1))}
                        }),
                        null)));

            } }

        private static StepMember GetVariableStepMember(string variableName)
        {
            return new StepMember(new CompoundFreezableStep(GetVariableStepFactory.Instance,
                new FreezableStepData(new Dictionary<string, StepMember>()
                {
                    {nameof(GetVariable<object>.VariableName), new StepMember(new VariableName(variableName)) }
                }), null
            ));
        }

        private sealed class StepMemberTestFunction : ITestFunction
        {
            public StepMemberTestFunction(string text, StepMember? expectedStepMember)
            {
                Name = text;
                ExpectedStepMember = expectedStepMember;
            }

            /// <inheritdoc />
            public string Name { get; }

            public StepMember? ExpectedStepMember { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var store = StepFactoryStore.CreateUsingReflection(typeof(Print<>));


                var parser = new StepMemberParser(store);

                var r = parser.TryParse(Name);

                if (ExpectedStepMember == null) //Expect failure
                {
                    r.ShouldBeFailure();
                }
                else
                {
                    r.ShouldBeSuccessful();

                    r.Value.Should().Be(ExpectedStepMember);
                }
            }
        }
    }
}