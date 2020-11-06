using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ArrayCountTests : StepTestBase<ArrayCount<string>, int>
    {
        /// <inheritdoc />
        public ArrayCountTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("short form",
                    "ArrayCount(Array = ['Hello','World'])",
                    2
                    );

                yield return new DeserializeCase("long form",
                    "Do: ArrayCount\nArray: ['Hello','World']",
                    2
                    );
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases {
            get
            {
                yield return new StepCase("Hello World",
                    new ArrayCount<string>
                    {
                        Array = new Array<string>
                        {
                            Elements = new List<IStep<string>>
                            {
                                Constant("Hello"),
                                Constant("World"),
                            }
                        }
                    },2);


                yield return new StepCase("Hello World multiline",
                    new ArrayCount<string>
                    {
                        Array = new Array<string>
                        {
                            Elements = new List<IStep<string>>
                            {
                                Constant($"Hello{Environment.NewLine}Hello"),
                                Constant($"World{Environment.NewLine}World"),
                            }
                        }
                    }, 2);

            } }

    }
}