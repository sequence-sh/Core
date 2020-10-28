using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class SplitStringTests : StepTestBase<SplitString, List<string>>
    {
        /// <inheritdoc />
        public SplitStringTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Split a string",
                    new SplitString()
                    {
                        String = Constant("Hello World"),
                        Delimiter = Constant(" ")
                    }, new List<string>(){"Hello", "World"}
                    );


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Split a string", "SplitString(String = 'Hello World', Delimiter = ' ')", new List<string>{"Hello", "World"});

            }

        }

    }
}