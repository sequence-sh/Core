using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class GetSubstringTests : StepTestBase<GetSubstring, string>
    {
        /// <inheritdoc />
        public GetSubstringTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Substring length 3",
                    new GetSubstring
                    {
                        String = Constant("Hello World"),
                        Index = Constant(1),
                        Length = Constant(3)
                    }, "ell"
                );


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Length 3",
                    "GetSubstring(String = 'Hello World', Index = 1, Length = 3", "ell");
            }

        }

    }
}