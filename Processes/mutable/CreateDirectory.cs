using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable.chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// Creates a new directory in the file system.
    /// </summary>
    public class CreateDirectory : Process
    {
        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetCreateDirectoryName();


        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            return TryConvertFreezeResult<TOutput, Unit>(TryFreeze());
        }

        private Result<IImmutableProcess<Unit>> TryFreeze()
        {
            if (string.IsNullOrWhiteSpace(Path))
                return Result.Failure<IImmutableProcess<Unit>>("Path must not be empty");

            return Result.Success<IImmutableProcess<Unit>>(new immutable.CreateDirectory(Path));
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput,Unit,TFinal,immutable.CreateDirectory,CreateDirectory>(this);
        }

        /// <summary>
        /// The path to the directory to create.
        /// </summary>
        [YamlMember(Order = 2)]
        [Required]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Path { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            yield break;
        }
    }
}