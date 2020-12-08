using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class DoXTimesTests : StepTestBase<DoXTimes, Unit>
    {
        /// <inheritdoc />
        public DoXTimesTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Print something three times",
                    new DoXTimes
                    {
                        Action = new Print<int>{Value = Constant(6)},
                        X = Constant(3)
                    }, Unit.Default, "6","6","6"
                    );

                yield return new StepCase("Run a sequence three times",
                    new DoXTimes
                    {
                        Action = new Sequence<Unit>()
                        {
                            InitialSteps = new List<IStep<Unit>>()
                            {
                                new Print<int> { Value = Constant(3) },
                            },
                            FinalStep = new Print<int> { Value = Constant(3) },
                        } ,
                        X = Constant(3)
                    }, Unit.Default, "3", "3", "3", "3", "3", "3"
                    );

            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Print something three times",
                    "DoXTimes Action: (Print Value: 6) X: 3", Unit.Default, "6","6","6");


            }

        }

    }
}