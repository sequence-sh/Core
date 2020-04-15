using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.EDR.Utilities.Processes.mutable.enumerations;

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

            var lines = frozenProcess.ExecuteUntyped();

            var l = await TestHelpers.AssertNoErrors(lines);

            CollectionAssert.Contains(l, "Word1");
            CollectionAssert.Contains(l, "Word2");
        }

        [Test]
        public void TestDeserializeProperties()
        {
            const string text = 
                @"
!Loop
For: !CSV
    CSVFilePath: Path
    InjectColumns:
      Number:
        Property: Number
      Term:
        Property: Term
    Delimiter: ','
    HasFieldsEnclosedInQuotes: true
Do: !EmitProcess
    Number: 1
    Term: T";
            
            var result = YamlHelper.TryMakeFromYaml(text);

            var process = result.AssertSuccess();
            
            Assert.IsInstanceOf<Loop>(process);

            var loop = process as Loop;
            Assert.IsNotNull(loop);
    
            Assert.IsInstanceOf<CSV>(loop.For);
            var csv = loop.For as CSV;
            Assert.IsNotNull(csv);

            Assert.AreEqual("Path", csv.CSVFilePath);
            Assert.AreEqual(",", csv.Delimiter);
            Assert.AreEqual(true, csv.HasFieldsEnclosedInQuotes);



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

            var lines = immutableProcess.ExecuteUntyped();

            var l = await TestHelpers.AssertNoErrors(lines);


            CollectionAssert.AreEquivalent(new[] {"Word1"}, l);
        }


    }
}
