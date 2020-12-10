using System.Collections.Generic;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class GetSubstringTests : StepTestBase<GetSubstring, StringStream>
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
                yield return new DeserializeCase("ArrayLength 3",
                    "GetSubstring String: 'Hello World' Index: 1 Length: 3", "ell");
            }

        }

    }
}