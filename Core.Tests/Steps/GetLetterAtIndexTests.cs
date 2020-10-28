using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class GetLetterAtIndexTests : StepTestBase<GetLetterAtIndex, string>
    {
        /// <inheritdoc />
        public GetLetterAtIndexTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Index is present", new GetLetterAtIndex()
                {
                    Index = Constant(1),
                    String = Constant("Hello")
                }, "e" );


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Index is present", "GetLetterAtIndex(Index = 1, String = 'Hello')", "e");

            }

        }

    }
}