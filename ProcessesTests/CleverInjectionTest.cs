using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.EDR.Utilities.Processes.mutable.chain;
using Reductech.EDR.Utilities.Processes.mutable.injection;
using YamlDotNet.Serialization;
using List = Reductech.EDR.Utilities.Processes.mutable.enumerations.List;
using RunExternalProcess = Reductech.EDR.Utilities.Processes.mutable.RunExternalProcess;

namespace Reductech.EDR.Utilities.Processes.Tests
{
    public class CleverInjectionTest
    {

        [Test]
        public void TestRegularInjection()
        {
            var process = new RunExternalProcess();

            var injector = new ProcessInjector(new List<(string element, Injection injection)>()
            {
                ("Pink", new Injection()
                {
                    Property = nameof(RunExternalProcess.ProcessPath)
                } )
            });

            var (isSuccess, _, error) = injector.Inject(process);

            Assert.IsTrue(isSuccess, error);

            Assert.AreEqual("Pink", process.ProcessPath);
        }

        [Test]
        public void TestNestedInjection()
        {
            var process = new Loop
            {
                Do = new RunExternalProcess()
            };

            var injector = new ProcessInjector(new List<(string element, Injection injection)>()
            {
                ("Pink", new Injection
                {
                    Property = $"{nameof(Loop.Do)}.{nameof(RunExternalProcess.ProcessPath)}"
                } )
            });

            var (isSuccess, _, error) = injector.Inject(process);

            Assert.IsTrue(isSuccess, error);

            Assert.AreEqual("Pink", ((RunExternalProcess)(process.Do)).ProcessPath);

        }


        [Test]
        public void TestArrayAccessInjection()
        {
            var process = new Loop
            {
                For = new List
                {
                    Members = new List<string>
                    {
                        "Orange"
                    }
                },
            };

            var injector = new ProcessInjector(new List<(string element, Injection injection)>()
            {
                ("Pink", new Injection
                {
                    Property = $"{nameof(Loop.For)}.{nameof(List.Members)}[0]"
                } )
            });

            var (isSuccess, _, error) = injector.Inject(process);

            Assert.IsTrue(isSuccess, error);

            Assert.AreEqual("Pink", ((List)(process.For)).Members.FirstOrDefault());
        }

        [Test]
        public void TestDictionaryAccessInjection()
        {
            var process = new DictionaryTestProcess
            {
                Parameters = new Dictionary<string, string>{{"Arg1", "Blue"}}
            };

            var injector = new ProcessInjector(new List<(string element, Injection injection)>()
            {
                ("Pink", new Injection
                {
                    Property = $"{nameof(DictionaryTestProcess.Parameters)}[Arg1]"
                } )
            });

            var (isSuccess, _, error) = injector.Inject(process);

            Assert.IsTrue(isSuccess, error);

            Assert.AreEqual("Pink", process.Parameters["Arg1"]);
        }
        /// <summary>
        /// Just used for this test
        /// </summary>
        private class DictionaryTestProcess : Process
        {
            [YamlMember]
            public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

            /// <inheritdoc />
            public override string GetReturnTypeInfo() => nameof(Unit);

            /// <inheritdoc />
            public override string GetName()
            {
                return "Dictionary";
            }

            /// <inheritdoc />
            public override Result<ImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
            {
                throw new System.NotImplementedException();
            }

            /// <inheritdoc />
            public override IEnumerable<string> GetRequirements()
            {
                yield break;
            }

            /// <inheritdoc />
            public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
            {
                throw new System.NotImplementedException();
            }
        }

    }
}
