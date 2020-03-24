using System.Threading.Tasks;
using NUnit.Framework;
using Reductech.EDR.Utilities.Processes.mutable;

namespace Reductech.EDR.Utilities.Processes.Tests
{
    public class YamlTest
    {
        [Test]
        public async Task TestDeserializeDefaults()
        {
            const string text = 
@"
!Sequence
Defaults: {Term: Word}    
Steps:
    - !EmitProcess
        Number: 1
    - !EmitProcess
        Number: 2
";

            var result = YamlHelper.TryMakeFromYaml(text);

            var process = result.AssertSuccess();

            var frozenProcess = process.TryFreeze(EmptySettings.Instance).AssertSuccess();

            var lines = frozenProcess.Execute();

            var l = await TestHelpers.AssertNoErrors(lines);

            CollectionAssert.Contains(l, "Word1");
            CollectionAssert.Contains(l, "Word2");
        }


        [Test]
        public async Task TestDeserializeIgnore()
        {
            //Ignore can either be a list or a property

            const string text = 
                @"
!EmitProcess
    Ignore:
        - &t Word
    Term: *t
    Ignore: &n 1
    Number: *n
    ";

            var result = YamlHelper.TryMakeFromYaml(text);

            var process = result.AssertSuccess();

            var immutableProcess = process.TryFreeze(EmptySettings.Instance).AssertSuccess();

            var lines = immutableProcess.Execute();

            var l = await TestHelpers.AssertNoErrors(lines);


            CollectionAssert.AreEquivalent(new[] {"Word1"}, l);
        }


    }
}
