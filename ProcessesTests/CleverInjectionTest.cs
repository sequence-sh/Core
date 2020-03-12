using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Reductech.EDR.Utilities.Processes.injection;
using List = Reductech.EDR.Utilities.Processes.enumerations.List;

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
            var process = new RunExternalProcess()
            { 
                Parameters = new Dictionary<string, string>{{"Arg1", "Blue"}}
            };

            var injector = new ProcessInjector(new List<(string element, Injection injection)>()
            {
                ("Pink", new Injection
                {
                    Property = $"{nameof(RunExternalProcess.Parameters)}[Arg1]" 
                } )
            });

            var (isSuccess, _, error) = injector.Inject(process);

            Assert.IsTrue(isSuccess, error);

            Assert.AreEqual("Pink", process.Parameters["Arg1"]);
        }

    }
}
