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
        public class DeserializeCase : CaseThatExecutes
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public DeserializeCase(string name, string scl, TOutput expectedOutput,
                params string[] expectedLoggedValues) : base(expectedLoggedValues)
            {
                Name = name;
                SCL = scl;
                ExpectedOutput = Maybe<TOutput>.From(expectedOutput);
            }

            // ReSharper disable once UnusedParameter.Local - needed to disambiguate constructor
            public DeserializeCase(string name, string scl, Unit _,
                params string[] expectedLoggedValues) : base(expectedLoggedValues)
            {
                Name = name;
                SCL = scl;
                ExpectedOutput = Maybe<TOutput>.None;
            }

            public string SCL { get; }

            public override string Name { get; }

            public Maybe<TOutput> ExpectedOutput { get; }

            /// <inheritdoc />
            public override async Task<IStep> GetStepAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
            {
                await Task.CompletedTask;

                var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep));

                testOutputHelper.WriteLine(SCL);

                var deserializeResult = SCLParsing.ParseSequence(SCL);

                deserializeResult.ShouldBeSuccessful(x => x.AsString);

                var freezeResult = deserializeResult.Value.TryFreeze(sfs);
                freezeResult.ShouldBeSuccessful(x => x.AsString);

                return freezeResult.Value;
            }



            /// <inheritdoc />
            public override void CheckOutputResult(Result<TOutput, IError> outputResult)
            {
                outputResult.ShouldBeSuccessful(x => x.AsString);

                if (outputResult.Value is Unit)
                    return;

                outputResult.Value.Should().BeEquivalentTo(ExpectedOutput.Value);
            }

            /// <inheritdoc />
            public override void CheckUnitResult(Result<Unit, IError> result)
            {
                result.ShouldBeSuccessful(x=>x.AsString);

                if (ExpectedOutput.HasValue) ExpectedOutput.Value.Should().Be(Unit.Default);
            }
        }
    }
}