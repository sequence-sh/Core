using CSharpFunctionalExtensions;
using FluentAssertions;
using OneOf;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.SpaceCompressor;

namespace Reductech.EDR.Core.TestHarness
{

public abstract partial class StepTestBase<TStep, TOutput>
{
    public class ExpectedOutput : OneOfBase<Unit, TOutput>
    {
        /// <inheritdoc />
        public ExpectedOutput(OneOf<Unit, TOutput> input) : base(input) { }

        public void CheckOutputResult(Result<TOutput, IError> outputResult)
        {
            outputResult.ShouldBeSuccessful(
                x => (x is SingleError se) ? $"{se.Message} in {se.Location}" : x.AsString
            );

            if (outputResult.Value is Unit)
                return;

            TryPickT1(out var expectedTOutput, out _).Should().BeTrue();

            if (outputResult.Value is string sActual && expectedTOutput is string sExpected)
                CompressSpaces(sActual).Should().Be(CompressSpaces(sExpected));
            else if (outputResult.Value is StringStream sActualStream
                  && expectedTOutput is StringStream sExpectedStream)
                CompressSpaces(sActualStream.GetString())
                    .Should()
                    .Be(CompressSpaces(sExpectedStream.GetString()));
            else
                outputResult.Value.Should().BeEquivalentTo(expectedTOutput);
        }

        public void CheckUnitResult(Result<Unit, IError> result)
        {
            result.ShouldBeSuccessful(
                x => (x is SingleError se) ? $"{se.Message} in {se.Location}" : x.AsString
            );

            if (IsT1)
                AsT1.Should().Be(Unit.Default);
        }

        public static explicit operator ExpectedOutput(Unit b) => new(b);
        public static implicit operator ExpectedOutput(TOutput b) => new(b);
    }
}

}
