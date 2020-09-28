using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Processes.Test
{
    public class DeserializationTests : DeserializationTestCases
    {
        public DeserializationTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;


        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(DeserializationTestCases))]
        public override void Test(string key) => base.Test(key);
    }
}
