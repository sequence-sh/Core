using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class PrintTests : StepTestBase<Print<string>, Unit>
    {
        /// <inheritdoc />
        public PrintTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Print something",
                    new Print<string>()
                    {
                        Value = Constant("Hello")
                    }, Unit.Default, "Hello"
                    );


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Named argument", "Print Value: 'Hello'", Unit.Default, "Hello");
                yield return new DeserializeCase("Ordered Argument", "Print 'Hello'", Unit.Default, "Hello");

            }

        }

    }
}