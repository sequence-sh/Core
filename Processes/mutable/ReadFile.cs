using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable.Chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable
{
    /// <summary>
    /// Reads a file and returns the contents.
    /// </summary>
    public class ReadFile : Process
    {
        /// <inheritdoc />
        public override string GetReturnTypeInfo()
        {
            return typeof(string).Name;
        }

        /// <inheritdoc />
        public override string GetName()
        {
            return ProcessNameHelper.GetReadFileName();
        }

        /// <summary>
        /// The path to the file.
        /// </summary>
        [YamlMember(Order = 1)]
        [Required]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string FilePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                return Result.Failure<IImmutableProcess<TOutput>>($"{nameof(FilePath)} must be set.");

            var process = new Immutable.ReadFile(FilePath);

            var r = TryConvertFreezeResult<TOutput, string>(process);

            return r;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            yield break;
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput, string, TFinal, Immutable.ReadFile, ReadFile>(this);
        }
    }
}
