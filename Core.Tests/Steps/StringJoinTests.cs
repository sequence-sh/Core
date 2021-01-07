using System.Collections.Generic;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class StringJoinTests : StepTestBase<StringJoin, StringStream>
    {
        /// <inheritdoc />
        public StringJoinTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Match some strings",
                    new StringJoin
                    {
                        Delimiter = Constant(", "),
                        Strings = Array( ("Hello"),  ("World") )
                    }, "Hello, World"
                    );

            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Match some strings",
                    "StringJoin Delimiter: ', ' Strings: ['Hello', 'World']"
                    ,"Hello, World"
                );



            }

        }

    }
}