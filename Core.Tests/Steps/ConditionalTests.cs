using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ConditionalTests : StepTestBase<Conditional, Unit>
    {
        /// <inheritdoc />
        public ConditionalTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase(
                    "If true print something",
                    "Conditional(Condition = true, ThenStep = Print(Value = 'Hello World'))",
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
                    new Conditional
                    {
                        Condition = Constant(true),
                        ThenStep = new Print<string>(){Value = Constant("Hello World")}
                    }, Unit.Default, "Hello World");

                yield return new StepCase("If false print nothing",
                    new Conditional
                    {
                        Condition = Constant(false),
                        ThenStep = new Print<string> { Value = Constant("Hello World") }
                    }, Unit.Default);

                yield return new StepCase("If false print something else",
                    new Conditional
                    {
                        Condition = Constant(false),
                        ThenStep = new Print<string> { Value = Constant("Hello World") },
                        ElseStep = new Print<string> { Value = Constant("Goodbye World") },
                    }, Unit.Default, "Goodbye World");
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases => ImmutableList<ErrorCase>.Empty;

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                yield return new SerializeCase("No Else",
                    new Conditional
                    {
                        Condition = Constant(true),
                        ThenStep = new Print<string>{Value = Constant("Hello World")}
                    }, "Conditional(Condition = True, ThenStep = Print(Value = 'Hello World'))"
                    );

                yield return new SerializeCase("Else",
                    new Conditional
                    {
                        Condition = Constant(true),
                        ThenStep = new Print<string> { Value = Constant("Hello World") },
                        ElseStep = new Print<string> { Value = Constant("Goodbye World") },
                    }, "Conditional(Condition = True, ElseStep = Print(Value = 'Goodbye World'), ThenStep = Print(Value = 'Hello World'))"
                    );
            }
        }
    }
}