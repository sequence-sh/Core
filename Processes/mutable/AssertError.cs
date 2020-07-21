using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable.Chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable
{
    /// <summary>
    /// Asserts that a particular process will produce an error.
    /// </summary>
    public class AssertError : Process
    {
        /// <summary>
        /// The process that is expected to fail.
        /// </summary>
        [Required]
        [YamlMember(Order = 1)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Process Process { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetAssertErrorName(Process?.GetName()??"");

        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            var r = TryFreeze(processSettings);
            return TryConvertFreezeResult<TOutput, Unit>(r);
        }

        private Result<IImmutableProcess<Unit>> TryFreeze(IProcessSettings processSettings)
        {
            if (Process == null)
                return Result.Failure<IImmutableProcess<Unit>>($"{nameof(Process)} is null.");

            var subProcessFreezeResult = Process.TryFreeze<Unit>(processSettings);

            if (subProcessFreezeResult.IsFailure) return subProcessFreezeResult;

            var r = new Immutable.AssertError( subProcessFreezeResult.Value);

            return r;
        }

        /// <inheritdoc />
        public override IEnumerable<Requirement> GetAllRequirements()
        {
            if (Process == null)
                return base.GetAllRequirements();

            return base.GetAllRequirements().Concat(Process.GetAllRequirements()).Distinct();
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput,Unit,TFinal,Immutable.AssertError,AssertError>(this);
        }
    }
}