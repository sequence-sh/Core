using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class RepeatXTimesTests : StepTestBase<RepeatXTimes, Unit>
    {
        /// <inheritdoc />
        public RepeatXTimesTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Print something three times",
                    new RepeatXTimes()
                    {
                        Action = new Print<int>(){Value = Constant(6)},
                        Number = Constant(3)
                    }, Unit.Default, "6","6","6"
                    );

            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Print something three times",
                    "RepeatXTimes(Action = Print(Value = 6), Number = 3)", Unit.Default, "6","6","6"
                    );


            }

        }

    }
}