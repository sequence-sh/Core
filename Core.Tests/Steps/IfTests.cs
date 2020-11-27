using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class IfTests : StepTestBase<If, Unit>
    {
        /// <inheritdoc />
        public IfTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase(
                    "If true print something",
                    "If(Condition = true, Then = Print(Value = 'Hello World'))",
                    Unit.Default,
                    "Hello World");
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("If true print something",
                    new If
                    {
                        Condition = Constant(true),
                        Then = new Print<string>(){Value = Constant("Hello World")}
                    }, Unit.Default, "Hello World");

                yield return new StepCase("If false print nothing",
                    new If
                    {
                        Condition = Constant(false),
                        Then = new Print<string> { Value = Constant("Hello World") }
                    }, Unit.Default);

                yield return new StepCase("If false print something else",
                    new If
                    {
                        Condition = Constant(false),
                        Then = new Print<string> { Value = Constant("Hello World") },
                        Else = new Print<string> { Value = Constant("Goodbye World") },
                    }, Unit.Default, "Goodbye World");
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                yield return new SerializeCase("No Else",
                    new If
                    {
                        Condition = Constant(true),
                        Then = new Print<string>{Value = Constant("Hello World")}
                    }, "If(Condition = True, Then = Print(Value = 'Hello World'))"
                    );

                yield return new SerializeCase("Else",
                    new If
                    {
                        Condition = Constant(true),
                        Then = new Print<string> { Value = Constant("Hello World") },
                        Else = new Print<string> { Value = Constant("Goodbye World") },
                    }, "If(Condition = True, Else = Print(Value = 'Goodbye World'), Then = Print(Value = 'Hello World'))"
                    );

                yield return CreateDefaultSerializeCase(true);
            }
        }
    }
}