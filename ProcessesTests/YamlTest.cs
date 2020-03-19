using System.Threading.Tasks;
using NUnit.Framework;

namespace Reductech.EDR.Utilities.Processes.Tests
{
    public class YamlTest
    {
        [Test]
        public async Task TestDeserializeIgnore()
        {
            const string text = 
                @"
!EmitProcess
    Ignore: &t Word
    Term: *t";

            var result = YamlHelper.TryMakeFromYaml(text);

            var process = result.AssertSuccess();

            var lines = process.Execute(new EmptySettings());

            var l = await TestHelpers.AssertNoErrors(lines);


            CollectionAssert.AreEquivalent(new[] {"Word"}, l);
        }


    }
}
