using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Reductech.EDR.Utilities.Processes.enumerations;
using Reductech.EDR.Utilities.Processes.injection;
using List = Reductech.EDR.Utilities.Processes.enumerations.List;

namespace Reductech.EDR.Utilities.Processes.Tests
{
    public class ForeachTests
    {
        private readonly IProcessSettings _processSettings = new EmptySettings();

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

            CollectionAssert.IsEmpty(forEachProcess.GetArgumentErrors());

            var resultList = forEachProcess.Execute(_processSettings);

            await foreach (var (isSuccess, _, value, error) in resultList)
            {
                Assert.IsTrue(isSuccess, error);
                realList.Add(value);
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

            var resultList = forEachProcess.Execute(_processSettings);

            await foreach (var (isSuccess, _, value, error) in resultList)
            {
                Assert.IsTrue(isSuccess, error);
                realList.Add(value);
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


            var resultList = forEachProcess.Execute(_processSettings);

            var realList = await TestHelpers.AssertNoErrors(resultList);

            CollectionAssert.AreEqual(expected, realList);
        }
    }
}
