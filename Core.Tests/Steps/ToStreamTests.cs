using System.Collections.Generic;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ToStreamTests : StepTestBase<StringToStream, DataStream>
    {
        /// <inheritdoc />
        public ToStreamTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get { yield return new StepCase("To stream test",
                new Print<string>
                {
                    Value = new StringFromStream
                    {
                        Stream = new StringToStream
                        {
                            String = Constant("Hello World")
                        }
                    }
                }, Unit.Default, "Hello World"
            );}
        }

    }
}