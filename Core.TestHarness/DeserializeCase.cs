using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Moq;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.TestHarness
{
    public abstract partial class StepTestBase<TStep, TOutput>
    {
        protected virtual IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield break;
            }
        }

        public IEnumerable<object?[]> DeserializeCaseNames => DeserializeCases.Select(x => new[] {x.Name});

        [Theory]
        [NonStaticMemberData(nameof(DeserializeCaseNames), true)]
        public async Task Should_behave_as_expected_when_deserialized(string deserializeCaseName)
        {
            await DeserializeCases.FindAndRunAsync(deserializeCaseName, TestOutputHelper);
        }

#pragma warning disable CA1034 // Nested types should not be visible
        public class DeserializeCase : ICaseThatExecutes
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public DeserializeCase(string name, string yaml, TOutput expectedOutput,
                params string[] expectedLoggedValues)
            {
                Name = name;
                Yaml = yaml;
                ExpectedOutput = Maybe<TOutput>.From(expectedOutput);
                ExpectedLoggedValues = expectedLoggedValues;
            }

            // ReSharper disable once UnusedParameter.Local - needed to disambiguate constructor
            public DeserializeCase(string name, string yaml, Unit _,
                params string[] expectedLoggedValues)
            {
                Name = name;
                Yaml = yaml;
                ExpectedOutput = Maybe<TOutput>.None;
                ExpectedLoggedValues = expectedLoggedValues;
            }

            public string Yaml { get; }

            public string Name { get; }

            public Maybe<TOutput> ExpectedOutput { get; }

            public IReadOnlyCollection<object> ExpectedLoggedValues { get; }

            public Dictionary<VariableName, object> InitialState { get; } = new Dictionary<VariableName, object>();

            public Dictionary<VariableName, object> ExpectedFinalState { get; } = new Dictionary<VariableName, object>();

            /// <inheritdoc />
            public Maybe<StepFactoryStore> StepFactoryStoreToUse { get; set; }

            /// <inheritdoc />
            public void AddExternalProcessRunnerAction(Action<Mock<IExternalProcessRunner>> action) => _externalProcessRunnerActions.Add(action);

            /// <inheritdoc />
            public void AddFileSystemAction(Action<Mock<IFileSystemHelper>> action) => _fileSystemActions.Add(action);

            private readonly List<Action<Mock<IExternalProcessRunner>>> _externalProcessRunnerActions = new List<Action<Mock<IExternalProcessRunner>>>();

            private readonly List<Action<Mock<IFileSystemHelper>>> _fileSystemActions = new List<Action<Mock<IFileSystemHelper>>>();


            /// <inheritdoc />
            public ISettings Settings { get; set; } = EmptySettings.Instance;

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
                var fileSystemMock = factory.Create<IFileSystemHelper>();

                foreach (var action in _externalProcessRunnerActions) action(externalProcessRunnerMock);

                foreach (var fileSystemAction in _fileSystemActions) fileSystemAction(fileSystemMock);

                var stateMonad = new StateMonad(logger, Settings, externalProcessRunnerMock.Object,  fileSystemMock.Object, StepFactoryStoreToUse.Unwrap(sfs));

                foreach (var (key, value) in InitialState)
                    stateMonad.SetVariable(key, value).ShouldBeSuccessful(x => x.AsString);

                if (ExpectedOutput.HasValue)
                {
                    var outputResult = await freezeResult.Value.Run<TOutput>(stateMonad, CancellationToken.None);

                    outputResult.ShouldBeSuccessful(x => x.AsString);
                    outputResult.Value.Should().BeEquivalentTo(ExpectedOutput.Value);
                }
                else
                {
                    var result = await freezeResult.Value.Run<Unit>(stateMonad, CancellationToken.None);

                    result.ShouldBeSuccessful(x => x.AsString);
                }

                logger.LoggedValues.Should().BeEquivalentTo(ExpectedLoggedValues);
                stateMonad.GetState().Should().BeEquivalentTo(ExpectedFinalState);
                factory.VerifyAll();
            }
        }
    }
}