using System.Collections.Generic;
using System.IO;
using System.Text;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class FromStreamTests : StepTestBase<StringFromStream, string>
    {
        /// <inheritdoc />
        public FromStreamTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("From Stream",
                    new StringFromStream
                    {
                        Stream = Constant(new DataStream(new MemoryStream( Encoding.UTF8.GetBytes( "Hello World" )), EncodingEnum.UTF8) )
                    },"Hello World"
                );
            }
        }
    }
}