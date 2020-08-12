using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Reductech.EDR.Processes.NewProcesses;
using Reductech.EDR.Processes.NewProcesses.General;
using Reductech.EDR.Processes.Tests.Extensions;
using Xunit;

namespace Reductech.EDR.Processes.Tests.NewProcessesTests
{
    public class ProcessTests : TestBase
    {
        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(ProcessTests))]
        public override void Test(string key) => base.Test(key);

        public const string HelloWorldString = "Hello World";
        public static readonly VariableName FooString = new VariableName("Foo");
        public static readonly VariableName BarString = new VariableName("Bar");

        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases
        {
            get
            {
                yield return new TestCase("Print 'Hello World'", new PrintProcessFactory.Print<string> {Value = new ConstantRunnableProcess<string>(HelloWorldString)},new []{HelloWorldString} );

                yield return new TestCase("Foo = Hello World; Print '<Foo>'",new SequenceProcessFactory.Sequence
                {
                    Steps = new List<IRunnableProcess<NewProcesses.Unit>>
                    {
                        new SetVariable<string>(FooString, new ConstantRunnableProcess<string>(HelloWorldString)),
                        new PrintProcessFactory.Print<string> {Value = new GetVariable<string>(FooString)}
                    }

                },new []{HelloWorldString});

                yield return new TestCase("Foo = Hello World; Bar = <Foo>; Print '<Bar>'",new SequenceProcessFactory.Sequence
                {
                    Steps = new List<IRunnableProcess<NewProcesses.Unit>>
                    {
                        new SetVariable<string>(FooString, new ConstantRunnableProcess<string>(HelloWorldString)),
                        new SetVariable<string>(BarString, new GetVariable<string>(FooString)),
                        new PrintProcessFactory.Print<string> {Value = new GetVariable<string>(BarString)}
                    }

                },new []{HelloWorldString});


                yield return new TestCase("Foo = 1 LessThan 2; Print '<Foo>'",new SequenceProcessFactory.Sequence
                {
                    Steps = new List<IRunnableProcess<NewProcesses.Unit>>
                    {
                        new SetVariable<bool>(FooString, new Compare<int>()
                        {
                            Left = new ConstantRunnableProcess<int>(1),
                            Operator = new ConstantRunnableProcess<CompareOperator>(CompareOperator.LessThan),
                            Right = new ConstantRunnableProcess<int>(2)
                        }),


                        new PrintProcessFactory.Print<bool> {Value = new GetVariable<bool>(FooString)}
                    }
                },new []{true.ToString()});
            }
        }



        private class TestCase : ITestCase
        {
            public TestCase(string expectedName, IRunnableProcess runnableProcess, IReadOnlyList<string> expectedLoggedValues)
            {
                RunnableProcess = runnableProcess;
                ExpectedLoggedValues = expectedLoggedValues;
                ExpectedName = expectedName;
            }

            public string ExpectedName { get; }

            /// <inheritdoc />
            public string Name => ExpectedName;

            public IRunnableProcess RunnableProcess { get; }


            public IReadOnlyList<string> ExpectedLoggedValues { get; }

            /// <inheritdoc />
            public void Execute()
            {
                RunnableProcess.Name.Should().Be(ExpectedName);


                var unfrozen = RunnableProcess.Unfreeze();

                var yaml = unfrozen.SerializeToYaml();

                var pfs = ProcessFactoryStore.CreateUsingReflection();


                var deserializeResult = NewProcesses.YamlHelper.DeserializeFromYaml(yaml, pfs);

                deserializeResult.ShouldBeSuccessful();

                deserializeResult.Value.ProcessName.Should().Be(unfrozen.ProcessName);

                var processContextResult = ProcessContext.TryCreate(deserializeResult.Value);
                processContextResult.ShouldBeSuccessful();

                var freezeResult = deserializeResult.Value.TryFreeze(processContextResult.Value);
                freezeResult.ShouldBeSuccessful();

                var runnable = freezeResult.Value.As<IRunnableProcess<NewProcesses.Unit>>();

                runnable.Should().NotBeNull("Process should unfreeze to runnable process of unit");

                var logger = new TestLogger();

                var state = new ProcessState(logger );

                var runResult = runnable.Run(state);

                runResult.ShouldBeSuccessful();

                logger.LoggedValues.Should().BeEquivalentTo(ExpectedLoggedValues);

            }
        }
    }

    public class TestLogger : ILogger
    {
        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (state is FormattedLogValues flv)
            {
                foreach (var formattedLogValue in flv)
                {
                    LoggedValues.Add(formattedLogValue.Value);
                }
            }
            else throw new NotImplementedException();
        }

        public List<object> LoggedValues = new List<object>();

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }

}
