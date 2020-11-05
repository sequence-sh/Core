using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ToStreamTests : StepTestBase<ToStream, Stream>
    {
        /// <inheritdoc />
        public ToStreamTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get { yield return new SequenceStepCase("To stream test",
                new Sequence
                {
                    Steps = new List<IStep<Unit>>
                    {
                        new Print<string>()
                        {
                            Value = new FromStream
                            {
                                Stream = new ToStream
                                {
                                    Text = Constant("Hello World")
                                }
                            }
                        }
                    }
                }, "Hello World"
            );}
        }

    }
}