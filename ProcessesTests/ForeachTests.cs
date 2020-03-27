using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.EDR.Utilities.Processes.mutable.enumerations;
using Reductech.EDR.Utilities.Processes.mutable.injection;
using Reductech.EDR.Utilities.Processes.output;
using List = Reductech.EDR.Utilities.Processes.mutable.enumerations.List;

namespace Reductech.EDR.Utilities.Processes.Tests
{
    public class ForeachTests
    {
        private readonly IProcessSettings _processSettings = EmptySettings.Instance;

        [Test]
        public async Task TestCSV()
        {
            var csv = string.Join("\r\n", "Text,Number,Color", "#Comment", "Correct,1,Red", "Horse,2,Yellow", "Battery,3,Green", "Staple,4,Blue");

            var expected = new List<string>
            {
                "Correct1", "Horse2", "Battery3", "Staple4"
            };

            var forEachProcess = new Loop
            {
                For = new CSV
                {
                    CSVText = csv,
                    CommentToken = "#",
                    Delimiter = ",",
                    InjectColumns = new Dictionary<string, Injection>
                    {
                        {
                            "Text",
                            new Injection
                            {
                                Property = nameof(EmitProcess.Term),
                                Regex = "(.+)",
                                Template = "$s"
                            }
                        }
                        ,
                        {
                            "Number",
                            new Injection
                            {
                                Property = nameof(EmitProcess.Number)
                            }
                        }
                        
                    }
                },

                Do = new EmitProcess()
            };

            var yaml = YamlHelper.ConvertToYaml(forEachProcess);

            Assert.IsNotEmpty(yaml);

            var realList = new List<string>();

            var immutableProcess = forEachProcess.TryFreeze(_processSettings).AssertSuccess();

            var output = immutableProcess.ExecuteUntyped();

            await foreach (var o in output)
            {
                Assert.IsTrue(o.OutputType != OutputType.Error, o.Text);
                if(o.OutputType == OutputType.Message)
                    realList.Add(o.Text);
            }

            CollectionAssert.AreEqual(expected, realList);

        }

        [Test]
        public async Task TestForeachProcess()
        {
            var list = new List<string>
            {
                "Correct", "Horse", "Battery", "Staple"
            };
            var expected = list.Select(s => $"'{s}'").ToList();

            var forEachProcess = new Loop
            {
                For = new List
                {
                    Members = list,
                    Inject = new List<Injection>
                    {
                        new Injection
                        {
                            Property = nameof(EmitProcess.Term),
                            Template = "'$s'"
                        }
                    }
                },
                Do = new EmitProcess()
            };

            var realList = new List<string>();

            var frozenProcess = forEachProcess.TryFreeze(_processSettings).AssertSuccess();


            var output = frozenProcess.ExecuteUntyped();

            await foreach (var o in output)
            {
                Assert.IsTrue(o.OutputType != OutputType.Error, o.Text);
                if(o.OutputType == OutputType.Message)
                    realList.Add(o.Text);
            }

            CollectionAssert.AreEqual(expected, realList);
        }

        [Test]
        public async Task TestForeachCasting()
        {
            var list = new List<string>
            {
                1.ToString(),2.ToString(),3.ToString(),4.ToString()
            };
            var expected = list.ToList();

            var forEachProcess = new Loop
            {
                For = new List
                {
                    Members = list,
                    Inject = new List<Injection>
                    {
                        new Injection
                        {
                            Property = nameof(EmitProcess.Number)
                        }
                    }
                },
                Do = new EmitProcess()
            };

            var resultList = forEachProcess.TryFreeze(_processSettings).AssertSuccess().ExecuteUntyped();

            var realList = await TestHelpers.AssertNoErrors(resultList);

            CollectionAssert.AreEqual(expected, realList);
        }
    }
}
