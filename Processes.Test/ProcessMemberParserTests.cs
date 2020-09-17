using System.Collections.Generic;
using FluentAssertions;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Serialization;
using Reductech.EDR.Processes.Test.Extensions;
using Xunit;
using Xunit.Abstractions;
using ITestCase = Reductech.EDR.Processes.Test.Extensions.ITestCase;

namespace Reductech.EDR.Processes.Test
{

    public class ProcessMemberParserTests : ProcessMemberParserTestCases
    {
        public ProcessMemberParserTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(ProcessMemberParserTestCases))]
        public override void Test(string key) => base.Test(key);
    }

    public class ProcessMemberParserTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases {
            get
            {
                yield return new ProcessMemberTestCase("true", new ProcessMember(new ConstantFreezableProcess(true)));
                yield return new ProcessMemberTestCase("True", new ProcessMember(new ConstantFreezableProcess(true)));
                yield return new ProcessMemberTestCase("1", new ProcessMember(new ConstantFreezableProcess(1)));
                yield return new ProcessMemberTestCase("\"Hello World\"", new ProcessMember(new ConstantFreezableProcess("Hello World")));
                yield return new ProcessMemberTestCase("Foo", null);
                yield return new ProcessMemberTestCase("true true", null);

                yield return new ProcessMemberTestCase("[true, false]", new ProcessMember(new List<IFreezableProcess>()
                {
                    new ConstantFreezableProcess(true),
                    new ConstantFreezableProcess(false)
                }));

                yield return new ProcessMemberTestCase("<foo> = 1", new ProcessMember(
                    new CompoundFreezableProcess(SetVariableProcessFactory.Instance,
                        new FreezableProcessData(new Dictionary<string, ProcessMember>
                        {
                            {nameof(SetVariable<int>.VariableName), new ProcessMember( new VariableName("foo"))},
                            {nameof(SetVariable<int>.Value), new ProcessMember(new ConstantFreezableProcess(1))}
                        }), null)));

                yield return new ProcessMemberTestCase("1 + 2", new ProcessMember(
                    new CompoundFreezableProcess(ApplyMathOperatorProcessFactory.Instance,
                        new FreezableProcessData(new Dictionary<string, ProcessMember>
                        {
                            {nameof(ApplyMathOperator.Left), new ProcessMember(new ConstantFreezableProcess(1))},
                            {nameof(ApplyMathOperator.Operator), new ProcessMember(new ConstantFreezableProcess(MathOperator.Add))},
                            {nameof(ApplyMathOperator.Right), new ProcessMember(new ConstantFreezableProcess(2))}
                        }), null
                        )));

                yield return new ProcessMemberTestCase("1 + <foo>", new ProcessMember(
                    new CompoundFreezableProcess(ApplyMathOperatorProcessFactory.Instance,
                        new FreezableProcessData(new Dictionary<string, ProcessMember>
                        {
                            {nameof(ApplyMathOperator.Left), new ProcessMember(new ConstantFreezableProcess(1))},
                            {nameof(ApplyMathOperator.Operator), new ProcessMember(new ConstantFreezableProcess(MathOperator.Add))},
                            {nameof(ApplyMathOperator.Right), new ProcessMember(new VariableName("foo"))}
                        }), null
                        )));

                yield return new ProcessMemberTestCase("<foo> + 2", new ProcessMember(
                    new CompoundFreezableProcess(ApplyMathOperatorProcessFactory.Instance,
                        new FreezableProcessData(new Dictionary<string, ProcessMember>
                        {
                            {nameof(ApplyMathOperator.Operator), new ProcessMember(new ConstantFreezableProcess(MathOperator.Add))},
                            {nameof(ApplyMathOperator.Left), new ProcessMember(new VariableName("foo"))},
                            {nameof(ApplyMathOperator.Right), new ProcessMember(new ConstantFreezableProcess(2))}
                        }), null
                        )));

            } }

        private sealed class ProcessMemberTestCase : ITestCase
        {
            public ProcessMemberTestCase(string text, ProcessMember? expectedProcessMember)
            {
                Name = text;
                ExpectedProcessMember = expectedProcessMember;
            }

            /// <inheritdoc />
            public string Name { get; }

            public ProcessMember? ExpectedProcessMember { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var parser = new ProcessMemberParser(ProcessFactoryStore.CreateUsingReflection());

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
