using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable.chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// Writes the output of a process to a file. Will overwrite if necessary.
    /// </summary>
    public class WriteFile : Process
    {
        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <summary>
        /// The process whose result is the text to be written.
        /// Should have a return type of string.
        /// </summary>
        [YamlMember(Order = 1 )]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Process Text { get; set; }

        /// <summary>
        /// The folder to create the file in.
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// The name of the file to write.
        /// </summary>
        public string FileName { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        public override string GetName()
        {
            return ProcessNameHelper.GetWriteFileProcessName(Text.GetName());
        }

        /// <inheritdoc />
        public override Result<ImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            return TryConvertFreezeResult<TOutput, Unit>(TryFreeze(processSettings));
        }


        private Result<ImmutableProcess<Unit>> TryFreeze(IProcessSettings processSettings)
        {
            var textFreezeResult = Text.TryFreeze<string>(processSettings);
            if (textFreezeResult.IsFailure)
                return textFreezeResult.ConvertFailure<ImmutableProcess<Unit>>();

            return Result.Success<ImmutableProcess<Unit>>(new immutable.WriteFile( textFreezeResult.Value, Folder, FileName));

        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            yield break;
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput,Unit,TFinal,immutable.WriteFile,WriteFile>(this);
        }
    }
}
