using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class DoesStringContainTests : StepTestBase<DoesStringContain, bool>
    {
        /// <inheritdoc />
        public DoesStringContainTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("True case sensitive",
                    new DoesStringContain
                    {
                        Superstring = Constant("Hello World"),
                        Substring = Constant("Hello")
                    }, true);

                yield return new StepCase("False case sensitive",
                    new DoesStringContain
                    {
                        Superstring = Constant("Hello World"),
                        Substring = Constant("hello")
                    }, false);

                yield return new StepCase("True case insensitive",
                    new DoesStringContain
                    {
                        Superstring = Constant("Hello World"),
                        Substring = Constant("hello"),
                        IgnoreCase = Constant(true)
                    }, true);

                yield return new StepCase("False case insensitive",
                    new DoesStringContain
                    {
                        Superstring = Constant("Hello World"),
                        Substring = Constant("Goodbye"),
                        IgnoreCase = Constant(true)
                    }, false);
            }
        }


    }
}