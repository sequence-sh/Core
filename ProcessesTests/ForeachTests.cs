using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Processes.enumerations;
using Processes.process;

namespace ProcessesTests
{
    public class ForeachTests
    {
        private readonly IProcessSettings _processSettings = new EmptySettings();

        [Test]
        public async Task TestCSV()
        {
            var csv = string.Join("\r\n", new []
            {
                "Text,Number,Color",
                "#Comment",
                "Correct,1,Red",
                "Horse,2,Yellow",
                "Battery,3,Green",
                "Staple,4,Blue",
            });

            var expected = new List<string>
            {
                "Correct1", "Horse2", "Battery3", "Staple4"
            };

            var forEachProcess = new ForEach
            {
                Enumeration = new CSVEnumeration
                {
                    CSVText = csv,
                    CommentToken = "#",
                    Delimiter = ",",
                    ColumnInjections = new List<ColumnInjection>
                    {
                        new ColumnInjection()
                        {
                            Header = "Text",
                            PropertyToInject = nameof(EmitProcess.Term)
                        },
                        new ColumnInjection()
                        {
                            Header = "Number",
                            PropertyToInject = nameof(EmitProcess.Number)
                        },
                    }
                },

                SubProcess = new EmitProcess()
            };

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

            var forEachProcess = new ForEach
            {
                Enumeration = new Collection
                {
                    Members = list,
                    Injections = new List<Injection>
                    {
                        new Injection
                        {
                            PropertyToInject = nameof(EmitProcess.Term),
                            Template = "'$s'",
                        }
                    }
                },
                SubProcess = new EmitProcess()
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

            var forEachProcess = new ForEach
            {
                Enumeration = new Collection
                {
                    Members = list,
                    Injections = new List<Injection>
                    {
                        new Injection
                        {
                            PropertyToInject = nameof(EmitProcess.Number)
                        }
                    }
                },
                SubProcess = new EmitProcess()
            };


            var resultList = forEachProcess.Execute(_processSettings);

            var realList = await TestHelpers.AssertNoErrors(resultList);

            CollectionAssert.AreEqual(expected, realList);
        }
    }
}
