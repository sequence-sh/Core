using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Gets a substring from a string.
    /// </summary>
    public sealed class GetSubstring : CompoundRunnableProcess<string>
    {
        /// <summary>
        /// The string to extract a substring from.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> String { get; set; } = null!;

        /// <summary>
        /// The index.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<int> Index { get; set; } = null!;

        /// <summary>
        /// The length of the substring to extract.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<int> Length { get; set; } = null!;


        /// <inheritdoc />
        public override Result<string> Run(ProcessState processState)
        {
            var str = String.Run(processState);
            if (str.IsFailure) return str;
            var index = Index.Run(processState);
            if (index.IsFailure) return index.ConvertFailure<string>();
            var length = Length.Run(processState);
            if (length.IsFailure) return length.ConvertFailure<string>();


            if (index.Value < 0 || index.Value >= str.Value.Length)
                return Result.Failure<string>("Index was outside the bounds of the string");

            return str.Value.Substring(index.Value, length.Value);
        }

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => GetSubstringProcessFactory.Instance;
    }

    /// <summary>
    /// Gets a substring from a string.
    /// </summary>
    public sealed class GetSubstringProcessFactory : SimpleRunnableProcessFactory<GetSubstring, string>
    {
        private GetSubstringProcessFactory() { }

        public static SimpleRunnableProcessFactory<GetSubstring, string> Instance { get; } = new GetSubstringProcessFactory();
    }
}