using System;
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
    /// Checks whether a particular file contains a particular string.
    /// </summary>
    public class DoesFileContain : Process
    {
        /// <summary>
        /// The path to the file to check.
        /// </summary>
        [Required]
        [YamlMember]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string FilePath { get; set; }

        /// <summary>
        /// The file must contain this string.
        /// </summary>

        [Required]
        [YamlMember]
        public string ExpectedContents { get; set; }

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Boolean);

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetAssertFileContainsProcessName();


        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            return TryConvertFreezeResult<TOutput, bool>(TryFreeze());
        }

        private Result<IImmutableProcess<bool>> TryFreeze()
        {
            var errors = new List<string>();

            if(string.IsNullOrWhiteSpace(FilePath)) errors.Add("FilePath is empty");
            if(string.IsNullOrWhiteSpace(ExpectedContents)) errors.Add("ExpectedContents is empty");

            if (errors.Any())
                return Result.Failure<IImmutableProcess<bool>>(string.Join("\r\n", errors));

            return Result.Success<IImmutableProcess<bool>>(new Immutable.DoesFileContain(FilePath, ExpectedContents));
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput,bool,TFinal,Immutable.DoesFileContain,DoesFileContain>(this);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            yield break;
        }
    }
}
