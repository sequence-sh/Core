using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Processes.Test
{
    public class ProcessTest : ProcessTestCases
    {
        public ProcessTest(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(ProcessTestCases))]
        public override void Test(string key) => base.Test(key);
    }
}
