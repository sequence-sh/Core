using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class IsEmptyTests : StepTestBase<IsEmpty<string>, bool>
    {
        /// <inheritdoc />
        public IsEmptyTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {

                yield return new DeserializeCase("short form empty",
                    "IsEmpty(Array = [])",
                    true
                );

                yield return new DeserializeCase("short form",
                    "IsEmpty(Array = ['Hello','World'])",
                    false
                );

                yield return new DeserializeCase("long form",
                    "Do: IsEmpty\nArray: ['Hello','World']",
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
                    new IsEmpty<string>()
                    {
                        Array = new Array<string>
                        {
                            Elements = new List<IStep<string>>
                            {
                                Constant("Hello"),
                                Constant("World"),
                            }
                        }
                    }, false);

                yield return new StepCase("Not Empty",
                    new IsEmpty<string>()
                    {
                        Array = new Array<string>
                        {
                            Elements = new List<IStep<string>>
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