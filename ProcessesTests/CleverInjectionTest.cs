using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable;
using Reductech.EDR.Processes.Mutable.Chain;
using Reductech.EDR.Processes.Mutable.Injections;
using YamlDotNet.Serialization;
using List = Reductech.EDR.Processes.Mutable.Enumerations.List;
using RunExternalProcess = Reductech.EDR.Processes.Mutable.RunExternalProcess;

namespace Reductech.EDR.Processes.Tests
{
    public class CleverInjectionTest
    {

        [Test]
        public void TestRegularInjection()
        {
            var process = new RunExternalProcess();

            var injections = new List<(object element, Injection injection)>
            {
                ("Pink", new Injection
                {
                    Property = nameof(RunExternalProcess.ProcessPath)
                })
            };

            var injector = new ProcessInjector(injections);
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

            var injector = new ProcessInjector(new List<(object element, Injection injection)>
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

            var injector = new ProcessInjector(new List<(object element, Injection injection)>
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

            var injector = new ProcessInjector(new List<(object element, Injection injection)>
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
            public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
            {
                throw new System.NotImplementedException();
            }

            /// <inheritdoc />
            public override IEnumerable<string> GetAllRequirements()
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
