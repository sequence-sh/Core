using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable.Chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable
{
    /// <summary>
    /// Deletes a file or a directory.
    /// </summary>
    public class DeleteItem : Process
    {
        /// <summary>
        /// The path to the file or directory to delete.
        /// </summary>
        [YamlMember(Order = 2)]
        [Required]

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Path { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetDeleteItemName();

        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            return TryConvertFreezeResult<TOutput, Unit>(TryFreeze());
        }

        private Result<IImmutableProcess<Unit>> TryFreeze()
        {
            if (string.IsNullOrWhiteSpace(Path))
                return Result.Failure<IImmutableProcess<Unit>>("File Path is empty");

            return new Immutable.DeleteItem(Path);
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput,Unit,TFinal,Immutable.DeleteItem,DeleteItem>(this);
        }
    }
}