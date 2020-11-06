using System;
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


                yield return new StepCase("Print something containing a newline",
                    new Print<string>()
                    {
                        Value = Constant($"Hello{Environment.NewLine}World")
                    }, Unit.Default, $"Hello{Environment.NewLine}World"
                    );


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Print something", "Print(Value = 'Hello')", Unit.Default, "Hello");

            }

        }

    }
}