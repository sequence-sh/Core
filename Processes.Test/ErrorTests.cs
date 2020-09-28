using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Processes.Test
{

    public class ErrorTests : ErrorTestCases
    {
        public ErrorTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(ErrorTestCases))]
        public override void Test(string key) => base.Test(key);
    }
}