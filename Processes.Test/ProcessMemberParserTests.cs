using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Processes.Test
{

    public class ProcessMemberParserTests : ProcessMemberParserTestCases
    {
        public ProcessMemberParserTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(ProcessMemberParserTestCases))]
        public override void Test(string key) => base.Test(key);
    }
}
