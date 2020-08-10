using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Reductech.EDR.Processes.NewProcesses;
using Reductech.EDR.Processes.NewProcesses.General;
using Reductech.EDR.Processes.Tests.Extensions;
using Xunit;

namespace Reductech.EDR.Processes.Tests.NewProcessesTests
{
    public class Yaml : TestBase
    {
        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(Yaml))]
        public override void Test(string key) => base.Test(key);

        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases
        {
            get
            {
                yield return new TestCase(new Print<string> {Value = new ConstantRunnableProcess<string>("Hello World")} );

                yield return new TestCase(new Sequence
                {
                    Steps = new List<IRunnableProcess<NewProcesses.Unit>>
                    {
                        new SetVariableRunnableProcess<string>("foo", new ConstantRunnableProcess<string>("Hello World")),
                        new Print<string> {Value = new GetVariableRunnableProcess<string>("foo")}
                    }

                });

                yield return new TestCase(new Sequence
                {
                    Steps = new List<IRunnableProcess<NewProcesses.Unit>>
                    {
                        new SetVariableRunnableProcess<string>("foo", new ConstantRunnableProcess<string>("Hello World")),
                        new SetVariableRunnableProcess<string>("bar", new GetVariableRunnableProcess<string>("foo")),
                        new Print<string> {Value = new GetVariableRunnableProcess<string>("bar")}
                    }

                });


                yield return new TestCase(new Sequence
                {
                    Steps = new List<IRunnableProcess<NewProcesses.Unit>>
                    {
                        new SetVariableRunnableProcess<bool>("foo", new Compare<int>()
                        {
                            Left = new ConstantRunnableProcess<int>(1),
                            Operator = new ConstantRunnableProcess<CompareOperator>(CompareOperator.LessThan),
                            Right = new ConstantRunnableProcess<int>(2)
                        }),


                        new Print<bool> {Value = new GetVariableRunnableProcess<bool>("foo")}
                    }
                });
            }
        }



        private class TestCase : ITestCase
        {
            public TestCase(IRunnableProcess runnableProcess)
            {
                RunnableProcess = runnableProcess;
            }

            /// <inheritdoc />
            public string Name => RunnableProcess.Name;

            public IRunnableProcess RunnableProcess { get; }

            /// <inheritdoc />
            public void Execute()
            {
                var unfrozen = RunnableProcess.Unfreeze();

                var yaml = unfrozen.SerializeToYaml();

                var pfs = ProcessFactoryStore.CreateUsingReflection();


                var deserializeResult = NewProcesses.YamlHelper.DeserializeFromYaml(yaml, pfs);

                deserializeResult.ShouldBeSuccessful();

                deserializeResult.Value.ProcessName.Should().Be(unfrozen.ProcessName);
            }
        }
    }

}
