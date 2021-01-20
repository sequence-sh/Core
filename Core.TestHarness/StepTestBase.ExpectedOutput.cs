using CSharpFunctionalExtensions;
using FluentAssertions;
using OneOf;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

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
            outputResult.ShouldBeSuccessful(x => x.ToString()!);

            if (outputResult.Value is Unit)
                return;

            TryPickT1(out var tOutput, out _).Should().BeTrue();

            //if (outputResult.Value is string sActual && tOutput is string sExpected)
            //    CompressSpaces(sActual).Should().Be(CompressSpaces(sExpected));
            //else
            outputResult.Value.Should().BeEquivalentTo(tOutput);
        }

        //private static readonly Regex SpaceRegex = new(@"\s+", RegexOptions.Compiled);
        //private static string CompressSpaces(string stepName) => SpaceRegex.Replace(stepName, " ");

        public void CheckUnitResult(Result<Unit, IError> result)
        {
            result.ShouldBeSuccessful(x => x.ToString()!);

            if (IsT1)
                AsT1.Should().Be(Unit.Default);
        }

        public static explicit operator ExpectedOutput(Unit b) => new(b);
        public static implicit operator ExpectedOutput(TOutput b) => new(b);
    }
}

}
