using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Reductech.EDR.Core.TestHarness
{
    public abstract partial class StepTestBase<TStep, TOutput>
    {
        protected abstract IEnumerable<DeserializeCase> DeserializeCases { get; }
        public IEnumerable<object?[]> DeserializeCaseNames => DeserializeCases.Select(x => new[] {x.Name});

        [Theory]
        [NonStaticMemberData(nameof(DeserializeCaseNames), true)]
        public async Task Should_behave_as_expected_when_deserialized(string deserializeCaseName)
        {
            await DeserializeCases.FindAndRunAsync(deserializeCaseName, TestOutputHelper);
        }

#pragma warning disable CA1034 // Nested types should not be visible
        public class DeserializeCase : ICaseThatRuns
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public DeserializeCase(string name, string yaml, TOutput expectedOutput,
                params object[] expectedLoggedValues)
            {
                Name = name;
                Yaml = yaml;
                ExpectedOutput = expectedOutput;
                ExpectedLoggedValues = expectedLoggedValues;
            }

            public string Yaml { get; }

            public string Name { get; }

            public TOutput ExpectedOutput { get; }

            public IReadOnlyCollection<object> ExpectedLoggedValues { get; }

            public Dictionary<VariableName, object> InitialState { get; } = new Dictionary<VariableName, object>();

            public Dictionary<VariableName, object> ExpectedFinalState { get; } = new Dictionary<VariableName, object>();

            /// <inheritdoc />
            public void AddExternalProcessRunnerAction(Action<Mock<IExternalProcessRunner>> action) => _externalProcessRunnerActions.Add(action);

            /// <inheritdoc />
            public ISettings Settings { get; set; } = EmptySettings.Instance;

            private readonly List<Action<Mock<IExternalProcessRunner>>> _externalProcessRunnerActions = new List<Action<Mock<IExternalProcessRunner>>>();

            /// <inheritdoc />
            public override string ToString() => Name;


            /// <inheritdoc />
            public async Task RunCaseAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
            {
                var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep));

                testOutputHelper.WriteLine(Yaml);

                var deserializeResult = YamlMethods.DeserializeFromYaml(Yaml, sfs);

                deserializeResult.ShouldBeSuccessful(x => x.AsString);

                var freezeResult = deserializeResult.Value.TryFreeze();
                freezeResult.ShouldBeSuccessful(x => x.AsString);

                var logger = new TestLogger();

                var factory = new MockRepository(MockBehavior.Strict);
                var externalProcessRunnerMock = factory.Create<IExternalProcessRunner>();


                foreach (var action in _externalProcessRunnerActions)
                {
                    action(externalProcessRunnerMock);
                }

                var stateMonad = new StateMonad(logger, Settings, externalProcessRunnerMock.Object, sfs);

                foreach (var (key, value) in InitialState)
                    stateMonad.SetVariable(key, value).ShouldBeSuccessful(x => x.AsString);

                var output = await freezeResult.Value.Run<TOutput>(stateMonad, CancellationToken.None);

                if(output.IsFailure)
                    throw new XunitException(output.Error.AsString);

                output.ShouldBeSuccessful(x => x.AsString);

                output.Value.Should().BeEquivalentTo(ExpectedOutput);

                logger.LoggedValues.Should().BeEquivalentTo(ExpectedLoggedValues);

                stateMonad.GetState().Should().BeEquivalentTo(ExpectedFinalState);

                factory.VerifyAll();
            }
        }
    }
}