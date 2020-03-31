using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// Asserts that a particular file contains a particular string.
    /// </summary>
    public class AssertFileContents : Process
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
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetAssertFileContainsProcess();

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            var errors = new List<string>();

            if(string.IsNullOrWhiteSpace(FilePath)) errors.Add("FilePath is empty");
            if(string.IsNullOrWhiteSpace(ExpectedContents)) errors.Add("ExpectedContents is empty");

            if (errors.Any())
                return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList(errors));

            return Result.Success<ImmutableProcess, ErrorList>(new ImmutableAssertFileContents(FilePath, ExpectedContents));
        }
    }
}
