using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class FirstIndexOfElementTests : StepTestBase<FindElement<string>, int>
    {
        /// <inheritdoc />
        public FirstIndexOfElementTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Simple case",
                    new FindElement<string>()
                    {
                        Array = Array("Hello", "World"),
                        Element = Constant("World")

                    },
                    1);

                yield return new StepCase("Duplicate Element",
                    new FindElement<string>
                    {
                        Array = Array("Hello", "World", "World"),
                        Element = Constant("World")

                    },
                    1);

                yield return new StepCase("Element not present",
                    new FindElement<string>
                    {
                        Array = Array("Hello", "World", "World"),
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
                yield return new DeserializeCase("Simple Case", "FirstIndexOfElement(Array = ['Hello', 'World'], Element = 'World')", 1);
            }

        }

    }
}