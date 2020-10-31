using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class TrimTests : StepTestBase<Trim, string>
    {
        /// <inheritdoc />
        public TrimTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Trim left", new Trim
                {
                    Side = Constant(TrimSide.Left),
                    String = Constant("  word  ")

                }, "word  " );


                yield return new StepCase("Trim right", new Trim
                {
                    Side = Constant(TrimSide.Right),
                    String = Constant("  word  ")

                }, "  word");

                yield return new StepCase("Trim both", new Trim
                {
                    Side = Constant(TrimSide.Both),
                    String = Constant("  word  ")

                }, "word");

            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Trim left", "Trim(Side = TrimSide.Left, String = '  word  ')", "word  ");

            }

        }

    }
}