using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Namotion.Reflection;
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
        protected abstract IEnumerable<StepCase> StepCases { get; }
        public IEnumerable<object?[]> StepCaseNames => StepCases.Select(x => new[] {x.Name});

        [Theory]
        [NonStaticMemberData(nameof(StepCaseNames), false)]
        public async Task Should_behave_as_expected_when_run(string stepCaseName)
        {
            await StepCases.FindAndRunAsync(stepCaseName, TestOutputHelper);
        }

        [Theory]
        [NonStaticMemberData(nameof(StepCaseNames), false)]
        public async Task Should_behave_as_expected_when_serialized_deserialized_and_executed(string stepCaseName)
        {
            await StepCases.FindAndRunAsync(stepCaseName, TestOutputHelper, StepCase.SerializeArgument);
        }

#pragma warning disable CA1034 // Nested types should not be visible
        public class StepCase : ICase, ICaseWithState
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public StepCase(string name, TStep step, TOutput expectedOutput, params object[] expectedLoggedValues)
            {
                Name = name;
                Step = step;
                ExpectedOutput = expectedOutput;
                ExpectedLoggedValues = expectedLoggedValues;
            }


            public string Name { get; }

            public TOutput ExpectedOutput { get; }

            public IReadOnlyCollection<object> ExpectedLoggedValues { get; }

            public Dictionary<VariableName, object> InitialState {get; } = new Dictionary<VariableName, object>();

            public Dictionary<VariableName, object> ExpectedFinalState { get; } = new Dictionary<VariableName, object>();

            public Action<Mock<IExternalProcessRunner>>? SetupMockExternalProcessRunner { get; set; }

            /// <inheritdoc />
            public override string ToString() => Name;

            public TStep Step { get; }


            public const string SerializeArgument = "Serialize";

            /// <summary>
            /// Serialize and Deserialize the step if required.
            /// </summary>
            private TStep GetStep(TStep step, string? extraArgument, ITestOutputHelper testOutputHelper,
                StepFactoryStore sfs)
            {
                if (extraArgument != SerializeArgument)
                    return step;

                var yaml = Step.Unfreeze().SerializeToYaml();

                testOutputHelper.WriteLine(yaml);

                var deserializeResult = YamlMethods.DeserializeFromYaml(yaml, sfs);

                deserializeResult.ShouldBeSuccessful(x => x.AsString);

                var freezeResult = deserializeResult.Value.TryFreeze();
                freezeResult.ShouldBeSuccessful(x => x.AsString);

                if (freezeResult.Value is TStep tStep)
                    return tStep;

                throw new XunitException($"'{yaml}' did not deserialize to a {typeof(TStep).GetDisplayName()}");
            }

            /// <inheritdoc />
            public virtual async Task RunCaseAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
            {
                testOutputHelper.WriteLine(Step.Name);

                var logger = new TestLogger();

                var factory = new MockRepository(MockBehavior.Strict);
                var externalProcessRunnerMock = factory.Create<IExternalProcessRunner>();

                var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep));

                SetupMockExternalProcessRunner?.Invoke(externalProcessRunnerMock);
                var stateMonad = new StateMonad(logger, EmptySettings.Instance, externalProcessRunnerMock.Object, sfs);

                foreach (var (key, value) in InitialState)
                    stateMonad.SetVariable(key, value).ShouldBeSuccessful(x => x.AsString);

                var step = GetStep(Step, extraArgument, testOutputHelper, sfs);

                var output = await step.Run<TOutput>(stateMonad, CancellationToken.None);

                output.ShouldBeSuccessful(x => x.AsString);

                output.Value.Should().BeEquivalentTo(ExpectedOutput);

                logger.LoggedValues.Should().BeEquivalentTo(ExpectedLoggedValues);

                stateMonad.GetState().Should().BeEquivalentTo(ExpectedFinalState);


                factory.VerifyAll();
            }
        }
    }
}