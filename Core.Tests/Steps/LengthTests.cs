using System;
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
    public class LengthTests : StepTestBase<ArrayLength<StringStream>, int>
    {
        /// <inheritdoc />
        public LengthTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("short form",
                    "ArrayLength Array: ['Hello','World']",
                    2
                    );
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases {
            get
            {
                yield return new StepCase("Hello World",
                    new ArrayLength<StringStream>
                    {
                        Array = new Array<StringStream>
                        {
                            Elements = new List<IStep<StringStream>>
                            {
                                Constant("Hello"),
                                Constant("World"),
                            }
                        }
                    },2);


                yield return new StepCase("Hello World multiline",
                    new ArrayLength<StringStream>
                    {
                        Array = new Array<StringStream>
                        {
                            Elements = new List<IStep<StringStream>>
                            {
                                Constant($"Hello{Environment.NewLine}Hello"),
                                Constant($"World{Environment.NewLine}World"),
                            }
                        }
                    }, 2);

            } }

    }
}