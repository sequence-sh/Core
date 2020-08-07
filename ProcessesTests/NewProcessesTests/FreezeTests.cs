using System.Collections.Generic;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Processes.NewProcesses;
using Reductech.EDR.Processes.NewProcesses.General;
using Reductech.EDR.Processes.Tests.Extensions;
using Xunit;

namespace Reductech.EDR.Processes.Tests.NewProcessesTests
{
    public class UnfreezeFreezeTests : TestBase
    {
        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(UnfreezeFreezeTests))]
        public override void Test(string key)
        {
            base.Test(key);
        }


        private class TestCase : ITestCase
        {
            public TestCase(IRunnableProcess process) => TestProcess = process;


            public IRunnableProcess TestProcess { get; }

            /// <inheritdoc />
            public string Name => TestProcess.Name;

            /// <inheritdoc />
            public void Execute()
            {
                var unfrozen = TestProcess.Unfreeze();


                var result = ProcessContext.TryCreate(unfrozen)
                    .Bind(x => unfrozen.TryFreeze(x));

                result.ShouldBeSuccessful();


                result.Value.Should().BeEquivalentTo(TestProcess);

                result.Value.Should().NotBe(TestProcess, "Freezing and Unfreezing should create a different object");
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases {
            get
            {
                yield return new TestCase(new ConstantRunnableProcess<string>("Hello World"));

                yield return new TestCase(new Sequence(){Steps = new List<IRunnableProcess<NewProcesses.Unit>>
                {
                    new SetVariableRunnableProcess<string>("MyVar", new ConstantRunnableProcess<string>("Hello World")),
                    new Print<string>
                    {
                        Value = new GetVariableRunnableProcess<string>("MyVar")
                    }
                }});


                //yield return new FreezeTest(new CompoundFreezableProcess(), );

            }
        }
    }
}
