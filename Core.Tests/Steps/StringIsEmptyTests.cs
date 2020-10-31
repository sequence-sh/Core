using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class StringIsEmptyTests : StepTestBase<StringIsEmpty, bool>
    {
        /// <inheritdoc />
        public StringIsEmptyTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("String is empty", new StringIsEmpty()
                {
                    String = Constant("")
                }, true );

                yield return new StepCase("String is not empty", new StringIsEmpty()
                {
                    String = Constant("Hello")
                }, false);

            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("String is empty", "StringIsEmpty(String = '')", true);
                yield return new DeserializeCase("String is not empty", "StringIsEmpty(String = 'Hello')", false);

            }

        }

    }
}