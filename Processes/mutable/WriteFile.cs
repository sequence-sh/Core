using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
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
        public override Result<ImmutableProcess> TryFreeze(IProcessSettings processSettings)
        {
            var textFreezeResult = Text.TryFreeze(processSettings);
            if (textFreezeResult.IsFailure)
                return textFreezeResult.ConvertFailure<ImmutableProcess>();

            if (textFreezeResult.Value is ImmutableProcess<string> ips)
                return Result.Success<ImmutableProcess>(new immutable.WriteFile( ips, Folder, FileName));

            return Result.Failure<ImmutableProcess>(new ErrorList(
                $"'{nameof(textFreezeResult.Value)}' must have return type 'string'."));

        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            yield break;
        }
    }
}
