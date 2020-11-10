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
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.TestHarness
{
    public abstract partial class StepTestBase<TStep, TOutput>
    {
        protected abstract IEnumerable<StepCase> StepCases { get; }
        public IEnumerable<object?[]> StepCaseNames => StepCases.Select(x => new[] {x.Name});

        [Theory]
        [NonStaticMemberData(nameof(StepCaseNames), true)]
        public async Task Should_behave_as_expected_when_run(string stepCaseName)
        {
            await StepCases.FindAndRunAsync(stepCaseName, TestOutputHelper);
        }

        [Theory]
        [NonStaticMemberData(nameof(StepCaseNames), true)]
        public async Task Should_behave_as_expected_when_serialized_deserialized_and_executed(string stepCaseName)
        {
            await StepCases.FindAndRunAsync(stepCaseName, TestOutputHelper, StepCase.SerializeArgument);
        }

#pragma warning disable CA1034 // Nested types should not be visible
        public class SequenceStepCase : StepCase
#pragma warning restore CA1034 // Nested types should not be visible
        {
            // ReSharper disable once UnusedParameter.Local - needed to disambiguate constructor
            public SequenceStepCase(string name, Sequence sequence, params string[] expectedLoggedValues) : base(name, sequence, Maybe<TOutput>.None,expectedLoggedValues)
            {
            }
        }

#pragma warning disable CA1034 // Nested types should not be visible
        public class StepCase : ICaseThatExecutes
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public StepCase(string name, TStep step, TOutput expectedOutput, params string[] expectedLoggedValues)
            :this(name, step, Maybe<TOutput>.From(expectedOutput), expectedLoggedValues)
            {
            }

            protected StepCase(string name, IStep step, Maybe<TOutput> expectedOutput, string[] expectedLoggedValues)
            {
                Name = name;
                Step = step;
                ExpectedOutput = expectedOutput;
                ExpectedLoggedValues = expectedLoggedValues.Select(CompressNewlines).ToList();
            }


            public string Name { get; }

            /// <summary>
            /// The expected output of the step.
            /// If this is not set. It is expected that a unit will be returned
            /// </summary>
            public Maybe<TOutput> ExpectedOutput { get; }

            public IReadOnlyCollection<string> ExpectedLoggedValues { get; }

            public Dictionary<VariableName, object> InitialState {get; } = new Dictionary<VariableName, object>();

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

            public IStep Step { get; }


            public const string SerializeArgument = "Serialize";

            /// <summary>
            /// Serialize and Deserialize the step if required.
            /// </summary>
            private async Task<IStep> GetStepAsync(IStep step, string? extraArgument, ITestOutputHelper testOutputHelper,
                StepFactoryStore sfs)
            {
                if (extraArgument != SerializeArgument)
                    return step;

                var yaml = await Step.Unfreeze().SerializeToYamlAsync(CancellationToken.None);

                testOutputHelper.WriteLine("");
                testOutputHelper.WriteLine("");
                testOutputHelper.WriteLine(yaml);


                var deserializeResult = YamlMethods.DeserializeFromYaml(yaml, sfs);

                deserializeResult.ShouldBeSuccessful(x => x.AsString);

                var freezeResult = deserializeResult.Value.TryFreeze();
                freezeResult.ShouldBeSuccessful(x => x.AsString);

                return freezeResult.Value;
            }

            /// <inheritdoc />
            public virtual async Task RunCaseAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
            {
                testOutputHelper.WriteLine(Step.Name);

                var logger = new TestLogger();

                var factory = new MockRepository(MockBehavior.Strict);
                var externalProcessRunnerMock = factory.Create<IExternalProcessRunner>();
                var fileSystemMock = factory.Create<IFileSystemHelper>();

                foreach (var action in _externalProcessRunnerActions) action(externalProcessRunnerMock);

                foreach (var fileSystemAction in _fileSystemActions) fileSystemAction(fileSystemMock);

                var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep));


                var step = await GetStepAsync(Step, extraArgument, testOutputHelper, sfs);

                var stateMonad = new StateMonad(logger, Settings, externalProcessRunnerMock.Object, fileSystemMock.Object, StepFactoryStoreToUse.Unwrap(sfs));

                foreach (var (key, value) in InitialState)
                    stateMonad.SetVariable(key, value).ShouldBeSuccessful(x => x.AsString);


                if (ExpectedOutput.HasValue)
                {
                    var outputResult = await step.Run<TOutput>(stateMonad, CancellationToken.None);

                    outputResult.ShouldBeSuccessful(x => x.AsString);
                    outputResult.Value.Should().BeEquivalentTo(ExpectedOutput.Value);
                }
                else
                {
                    var result = await step.Run<Unit>(stateMonad, CancellationToken.None);
                    result.ShouldBeSuccessful(x=>x.AsString);
                }

                logger.LoggedValues.Select(x=> CompressNewlines(x.ToString()!)) .Should().BeEquivalentTo(ExpectedLoggedValues);
                stateMonad.GetState().Should().BeEquivalentTo(ExpectedFinalState);

                factory.VerifyAll();
            }
        }
    }
}