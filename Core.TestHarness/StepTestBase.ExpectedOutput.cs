using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.SpaceCompressor;

namespace Reductech.EDR.Core.TestHarness;

public abstract partial class StepTestBase<TStep, TOutput>
{
    public record ExpectedUnitOutput : ExpectedOutput
    {
        private ExpectedUnitOutput() { }
        public static ExpectedUnitOutput Instance { get; } = new();

        /// <inheritdoc />
        public override void CheckOutputResult(Result<TOutput, IError> outputResult)
        {
            outputResult.ShouldBeSuccessful();

            outputResult.Value.Should().Be(Unit.Default);
        }

        /// <inheritdoc />
        public override void CheckUnitResult(Result<Unit, IError> result)
        {
            result.ShouldBeSuccessful();
        }

        public static explicit operator ExpectedUnitOutput(Unit _) => Instance;
    }

    public record ExpectedValueOutput(TOutput Expected) : ExpectedOutput
    {
        /// <inheritdoc />
        public override void CheckOutputResult(Result<TOutput, IError> outputResult)
        {
            outputResult.ShouldBeSuccessful();

            if (outputResult.Value is string sActual && Expected is string sExpected)
                CompressSpaces(sActual).Should().Be(CompressSpaces(sExpected));
            else if (outputResult.Value is StringStream sActualStream
                  && Expected is StringStream sExpectedStream)
                CompressSpaces(sActualStream.GetString())
                    .Should()
                    .Be(CompressSpaces(sExpectedStream.GetString()));
            else
                outputResult.Value.Should().BeEquivalentTo(Expected);
        }

        /// <inheritdoc />
        public override void CheckUnitResult(Result<Unit, IError> result)
        {
            result.ShouldBeSuccessful();

            Expected.Should().Be(Unit.Default);
        }

        public static implicit operator ExpectedValueOutput(TOutput b) => new(b);
    }

    public abstract record ExpectedOutput
    {
        public abstract void CheckOutputResult(Result<TOutput, IError> outputResult);

        public abstract void CheckUnitResult(Result<Unit, IError> result);
    }
}
