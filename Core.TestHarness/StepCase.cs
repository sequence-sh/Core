using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

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
        public async Task Should_behave_as_expected_when_serialized_deserialized_and_run(string stepCaseName)
        {
            await StepCases.FindAndRunAsync(stepCaseName, TestOutputHelper, StepCase.SerializeArgument);
        }


#pragma warning disable CA1034 // Nested types should not be visible
        public class StepCase : CaseThatExecutes
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public StepCase(string name, TStep step, TOutput expectedOutput, params string[] expectedLoggedValues)
            :this(name, step, Maybe<TOutput>.From(expectedOutput), expectedLoggedValues)
            {
            }


            public StepCase(string name, IStep<Unit> step, Unit _, params string[] expectedLoggedValues)
                : this(name, step, Maybe<TOutput>.None,expectedLoggedValues)
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


            public const string SerializeArgument = "SerializeAsync";

            /// <inheritdoc />
            public override async Task<IStep> GetStepAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
            {
                await ValueTask.CompletedTask;

                if (extraArgument != SerializeArgument)
                    return Step;

                var scl = Step.Serialize();

                testOutputHelper.WriteLine("");
                testOutputHelper.WriteLine("");
                testOutputHelper.WriteLine(scl);

                var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep));

                var deserializeResult = SCLParsing.ParseSequence(scl);

                deserializeResult.ShouldBeSuccessful(x => x.AsString);

                var freezeResult = deserializeResult.Value.TryFreeze(sfs);
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