using System.Collections.Generic;
using FluentAssertions;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Serialization;
using Reductech.EDR.Processes.Test.Extensions;
using Xunit.Abstractions;
using ITestCase = Reductech.EDR.Processes.Test.Extensions.ITestCase;

namespace Reductech.EDR.Processes.Test
{
    public class ProcessMemberParserTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases {
            get
            {
                yield return new ProcessMemberTestCase("true", new StepMember(new ConstantFreezableStep(true)));
                yield return new ProcessMemberTestCase("True", new StepMember(new ConstantFreezableStep(true)));
                yield return new ProcessMemberTestCase("1", new StepMember(new ConstantFreezableStep(1)));
                yield return new ProcessMemberTestCase("\"Hello World\"", new StepMember(new ConstantFreezableStep("Hello World")));
                yield return new ProcessMemberTestCase("Foo", null);
                yield return new ProcessMemberTestCase("true true", null);

                yield return new ProcessMemberTestCase("not (true)",
                    new StepMember(
                        new CompoundFreezableStep(NotStepFactory.Instance,
                            new FreezableStepData(new Dictionary<string, StepMember>()
                            {
                                {nameof(Not.Boolean), new StepMember(new ConstantFreezableStep(true))}
                            }), null
                        ))

                );

                yield return new ProcessMemberTestCase("[true, false]", new StepMember(new List<IFreezableStep>()
                {
                    new ConstantFreezableStep(true),
                    new ConstantFreezableStep(false)
                }));

                yield return new ProcessMemberTestCase("<foo> = 1", new StepMember(
                    new CompoundFreezableStep(SetVariableStepFactory.Instance,
                        new FreezableStepData(new Dictionary<string, StepMember>
                        {
                            {nameof(SetVariable<int>.VariableName), new StepMember( new VariableName("foo"))},
                            {nameof(SetVariable<int>.Value), new StepMember(new ConstantFreezableStep(1))}
                        }), null)));

                yield return new ProcessMemberTestCase("1 + 2", new StepMember(
                    new CompoundFreezableStep(ApplyMathOperatorStepFactory.Instance,
                        new FreezableStepData(new Dictionary<string, StepMember>
                        {
                            {nameof(ApplyMathOperator.Left), new StepMember(new ConstantFreezableStep(1))},
                            {nameof(ApplyMathOperator.Operator), new StepMember(new ConstantFreezableStep(MathOperator.Add))},
                            {nameof(ApplyMathOperator.Right), new StepMember(new ConstantFreezableStep(2))}
                        }), null
                    )));

                yield return new ProcessMemberTestCase("1 + <foo>", new StepMember(
                    new CompoundFreezableStep(ApplyMathOperatorStepFactory.Instance,
                        new FreezableStepData(new Dictionary<string, StepMember>
                        {
                            {nameof(ApplyMathOperator.Left), new StepMember(new ConstantFreezableStep(1))},
                            {nameof(ApplyMathOperator.Operator), new StepMember(new ConstantFreezableStep(MathOperator.Add))},
                            {nameof(ApplyMathOperator.Right), GetVariableProcessMember("foo")}
                        }), null
                    )));

                yield return new ProcessMemberTestCase("<foo> + 2", new StepMember(
                    new CompoundFreezableStep(ApplyMathOperatorStepFactory.Instance,
                        new FreezableStepData(new Dictionary<string, StepMember>
                        {
                            {nameof(ApplyMathOperator.Operator), new StepMember(new ConstantFreezableStep(MathOperator.Add))},
                            {nameof(ApplyMathOperator.Left), GetVariableProcessMember("foo")},
                            {nameof(ApplyMathOperator.Right), new StepMember(new ConstantFreezableStep(2))}
                        }), null
                    )));

                yield return new ProcessMemberTestCase("Print(Value = 1)", new StepMember(
                    new CompoundFreezableStep(PrintStepFactory.Instance,
                        new FreezableStepData(new Dictionary<string, StepMember>
                        {
                            {nameof(Print<int>.Value), new StepMember(new ConstantFreezableStep(1))}
                        }),
                        null)));

            } }

        private static StepMember GetVariableProcessMember(string variableName)
        {
            return new StepMember(new CompoundFreezableStep(GetVariableStepFactory.Instance,
                new FreezableStepData(new Dictionary<string, StepMember>()
                {
                    {nameof(GetVariable<object>.VariableName), new StepMember(new VariableName(variableName)) }
                }), null
            ));
        }

        private sealed class ProcessMemberTestCase : ITestCase
        {
            public ProcessMemberTestCase(string text, StepMember? expectedProcessMember)
            {
                Name = text;
                ExpectedProcessMember = expectedProcessMember;
            }

            /// <inheritdoc />
            public string Name { get; }

            public StepMember? ExpectedProcessMember { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var store = StepFactoryStore.CreateUsingReflection(typeof(Print<>));


                var parser = new StepMemberParser(store);

                var r = parser.TryParse(Name);

                if (ExpectedProcessMember == null) //Expect failure
                {
                    r.ShouldBeFailure();
                }
                else
                {
                    r.ShouldBeSuccessful();

                    r.Value.Should().Be(ExpectedProcessMember);
                }
            }
        }
    }
}