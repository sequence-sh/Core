using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;


namespace Reductech.EDR.Core.Tests.Steps
{
    public class IsEmptyTests : StepTestBase<ArrayIsEmpty<StringStream>, bool>
    {
        /// <inheritdoc />
        public IsEmptyTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {

                yield return new DeserializeCase("empty array",
                    "ArrayIsEmpty Array: []",
                    true
                );

                yield return new DeserializeCase("two element",
                    "ArrayIsEmpty Array: ['Hello','World']",
                    false
                );
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Empty",
                    new ArrayIsEmpty<StringStream>()
                    {
                        Array = new Array<StringStream>
                        {
                            Elements = new List<IStep<StringStream>>
                            {
                                Constant("Hello"),
                                Constant("World"),
                            }
                        }
                    }, false);

                yield return new StepCase("Not Empty",
                    new ArrayIsEmpty<StringStream>()
                    {
                        Array = new Array<StringStream>
                        {
                            Elements = new List<IStep<StringStream>>
                            {
                                Constant("Hello"),
                                Constant("World"),
                            }
                        }
                    }, false);
            }
        }


    }
}