using System.Collections.Generic;
using FluentAssertions;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.Core;


namespace Reductech.EDR.Core.Tests
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

        private static IFreezableStep Constant(object value) => new ConstantFreezableStep(value);

        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCase> TestCases {
            get
            {
                yield return new StepMemberTestFunction("true", new StepMember(Constant(true)));
                yield return new StepMemberTestFunction("True", new StepMember(Constant(true)));
                yield return new StepMemberTestFunction("1", new StepMember(Constant(1)));
                yield return new StepMemberTestFunction("\"Hello World\"", new StepMember(Constant("Hello World")));
                //yield return new StepMemberTestFunction("Foo", null);
                yield return new StepMemberTestFunction("true true", null);

                yield return new StepMemberTestFunction("not (true)",
                    new StepMember(

                        NotStepFactory.CreateFreezable(Constant(true))
                        ));

                yield return new StepMemberTestFunction("[true, false]", new StepMember(new List<IFreezableStep>()
                {
                    Constant(true),
                    Constant(false)
                }));

                yield return new StepMemberTestFunction("<foo> = 1", new StepMember(
                    SetVariableStepFactory.CreateFreezable(new VariableName("foo"),Constant(1) )));

                yield return new StepMemberTestFunction("1 + 2", new StepMember(
                    ApplyMathOperatorStepFactory.CreateFreezable(Constant(1), Constant(MathOperator.Add), Constant(2) )));

                yield return new StepMemberTestFunction("(1 + 2)", new StepMember(
                        ApplyMathOperatorStepFactory.CreateFreezable(Constant(1), Constant(MathOperator.Add), Constant(2))));

                yield return new StepMemberTestFunction("1 + <foo>", new StepMember(
                    ApplyMathOperatorStepFactory.CreateFreezable(Constant(1), Constant(MathOperator.Add), GetVariable("foo"))));


                yield return new StepMemberTestFunction("<foo> + 2", new StepMember(
                    ApplyMathOperatorStepFactory.CreateFreezable(GetVariable("foo"), Constant(MathOperator.Add), Constant(2) )));


                yield return new StepMemberTestFunction("Print(Value = 1)", new StepMember(
                    new CompoundFreezableStep(PrintStepFactory.Instance,

                        new FreezableStepData(new Dictionary<string, IFreezableStep>
                        {
                            {nameof(Print<int>.Value), Constant(1)}
                        }, null, null),
                        null)));

            } }

        private static IFreezableStep GetVariable(string variableName) => GetVariableStepFactory.CreateFreezable(new VariableName(variableName));

        private sealed class StepMemberTestFunction : ITestBaseCase
        {
            public StepMemberTestFunction(string text, StepMember? expectedStepMember)
            {
                Name = text;
                ExpectedStepMember = expectedStepMember;
            }

            /// <inheritdoc />
            public string Name { get; }

            private StepMember? ExpectedStepMember { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var store = StepFactoryStore.CreateUsingReflection(typeof(Print<>));

                var parser = new StepMemberParser(store);

                var r = parser.TryParse(Name, Mark.Empty, Mark.Empty);

                if (ExpectedStepMember == null) //Expect failure
                {
                    r.ShouldBeFailure();
                }
                else
                {
                    r.ShouldBeSuccessful(x=>x.ToString()!);

                    r.Value.Should().Be(ExpectedStepMember);
                }
            }
        }
    }
}