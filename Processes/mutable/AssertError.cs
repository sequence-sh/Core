using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable.chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
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
        public override Result<ImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            var r = TryFreeze(processSettings);
            return TryConvertFreezeResult<TOutput, Unit>(r);
        }

        private Result<ImmutableProcess<Unit>> TryFreeze(IProcessSettings processSettings)
        {
            if (Process == null)
                return Result.Failure<ImmutableProcess<Unit>>($"{nameof(Process)} is null.");

            var subProcessFreezeResult = Process.TryFreeze<Unit>(processSettings);

            if (subProcessFreezeResult.IsFailure) return subProcessFreezeResult;

            var r = new immutable.AssertError( subProcessFreezeResult.Value);

            return r;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            if (Process == null)
                return Enumerable.Empty<string>();

            return Process.GetRequirements();
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput,Unit,TFinal,immutable.AssertError,AssertError>(this);
        }
    }
}