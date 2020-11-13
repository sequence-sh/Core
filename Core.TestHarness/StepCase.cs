using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
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
        public class StepCase : CaseThatExecutes
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public StepCase(string name, TStep step, TOutput expectedOutput, params string[] expectedLoggedValues)
            :this(name, step, Maybe<TOutput>.From(expectedOutput), expectedLoggedValues)
            {
            }

            protected StepCase(string name, IStep step, Maybe<TOutput> expectedOutput, string[] expectedLoggedValues) : base(expectedLoggedValues.Select(CompressNewlines).ToList())
            {
                Name = name;
                Step = step;
                ExpectedOutput = expectedOutput;
            }


            public override string Name { get; }

            /// <summary>
            /// The expected output of the step.
            /// If this is not set. It is expected that a unit will be returned
            /// </summary>
            public Maybe<TOutput> ExpectedOutput { get; }


            /// <inheritdoc />
            public override string ToString() => Name;

            public IStep Step { get; }


            public const string SerializeArgument = "Serialize";

            /// <inheritdoc />
            public override async Task<IStep> GetStepAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
            {
                if (extraArgument != SerializeArgument)
                    return Step;

                var yaml = await Step.Unfreeze().SerializeToYamlAsync(CancellationToken.None);

                testOutputHelper.WriteLine("");
                testOutputHelper.WriteLine("");
                testOutputHelper.WriteLine(yaml);


                var deserializeResult = YamlMethods.DeserializeFromYaml(yaml, StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep)));

                deserializeResult.ShouldBeSuccessful(x => x.AsString);

                var freezeResult = deserializeResult.Value.TryFreeze();
                freezeResult.ShouldBeSuccessful(x => x.AsString);

                return freezeResult.Value;
            }



            /// <inheritdoc />
            public override void CheckOutputResult(Result<TOutput, IError> result)
            {
                result.ShouldBeSuccessful(x => x.AsString);
                if(result.Value is Unit)
                    return;

                result.Value.Should().BeEquivalentTo(ExpectedOutput.Value);
            }

            /// <inheritdoc />
            public override void CheckUnitResult(Result<Unit, IError> result)
            {
                result.ShouldBeSuccessful(x => x.AsString);

                if (ExpectedOutput.HasValue)
                    ExpectedOutput.Value.Should().Be(Unit.Default);
            }
        }
    }
}