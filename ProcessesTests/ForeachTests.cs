using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using NUnit.Framework;
using Processes.enumerations;
using Processes.process;
using YamlDotNet.Serialization;

namespace ProcessesTests
{
    public class ForeachTests
    {
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
                            PropertyToInject = nameof(EmitTermProcess.Term),
                            Template = "'$s'",
                        }
                    }
                },
                SubProcess = new EmitTermProcess()
            };

            var realList = new List<string>();

            var resultList = forEachProcess.Execute();

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
                            PropertyToInject = nameof(EmitIntProcess.Number)
                        }
                    }
                },
                SubProcess = new EmitIntProcess()
            };

            var realList = new List<string>();

            var resultList = forEachProcess.Execute();

            await foreach (var (isSuccess, _, value, error) in resultList)
            {
                Assert.IsTrue(isSuccess, error);
                realList.Add(value);
            }

            CollectionAssert.AreEqual(expected, realList);
        }

        private class EmitIntProcess : Process
        {
            [UsedImplicitly]
            [YamlMember]
            [Required]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
            public int Number { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

            public override IEnumerable<string> GetArgumentErrors()
            {
                yield break;
            }

            public override string GetName()
            {
                return "Emit Number";
            }

#pragma warning disable 1998
            public override async IAsyncEnumerable<Result<string>> Execute()
#pragma warning restore 1998
            {
                yield return Result.Success(Number.ToString());
            }
        }

        private class EmitTermProcess : Process
        {
            [UsedImplicitly]
            [YamlMember]
            [Required]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
            public string Term { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

            public override IEnumerable<string> GetArgumentErrors()
            {
                yield break;
            }

            public override string GetName()
            {
                return "Emit Term";
            }

#pragma warning disable 1998
            public override async IAsyncEnumerable<Result<string>> Execute()
#pragma warning restore 1998
            {
                yield return Result.Success(Term);
            }
        }

    }
}
