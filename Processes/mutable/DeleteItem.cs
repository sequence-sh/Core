using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
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
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            if (string.IsNullOrWhiteSpace(Path))
                return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList("File Path is empty"));

            return Result.Success<ImmutableProcess, ErrorList>(new immutable.DeleteItem(Path));
        }
    }
}