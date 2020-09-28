using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Processes.Test
{
    public class SerializationTests : SerializationTestCases
    {
        public SerializationTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(SerializationTestCases))]
        public override void Test(string key) => base.Test(key);
    }
}
