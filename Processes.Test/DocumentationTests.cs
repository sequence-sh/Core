using Reductech.EDR.Processes.General;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Processes.Test
{
    public class DocumentationTests : DocumentationTestCases
    {
        public DocumentationTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;


        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(DocumentationTestCases))]
        public override void Test(string key) => base.Test(key);
    }
}
