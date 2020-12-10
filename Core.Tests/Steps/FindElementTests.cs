using System.Collections.Generic;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class FindElementTests : StepTestBase<FindElement<StringStream>, int>
    {
        /// <inheritdoc />
        public FindElementTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Simple case",
                    new FindElement<StringStream>()
                    {
                        Array = Array( ("Hello") , ("World") ),
                        Element = Constant("World")

                    },
                    1);

                yield return new StepCase("Duplicate Element",
                    new FindElement<StringStream>
                    {
                        Array = Array( ("Hello") ,  ("World") ,  ("World")),
                        Element = Constant("World")

                    },
                    1);

                yield return new StepCase("Element not present",
                    new FindElement<StringStream>
                    {
                        Array = Array(("Hello") , ("World") , ("World")),
                        Element = Constant("Mark")

                    },
                    -1);


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Simple Case", "FindElement Array: ['Hello', 'World'] Element: 'World'", 1);
            }

        }

    }
}