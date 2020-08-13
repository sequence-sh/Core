using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// Gets the letters that appears at a specific index
    /// </summary>
    public sealed class GetLetterAtIndex : CompoundRunnableProcess<string>
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

        /// <inheritdoc />
        public override Result<string> Run(ProcessState processState)
        {
            var index = Index.Run(processState);
            if (index.IsFailure) return index.ConvertFailure<string>();

            var str = String.Run(processState);
            if (str.IsFailure) return str;

            if (index.Value < 0 || index.Value >= str.Value.Length)
                return Result.Failure<string>("Index was outside the bounds of the string");

            return str.Value[index.Value].ToString();
        }

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => GetLetterAtIndexProcessFactory.Instance;
    }

    /// <summary>
    /// Gets the letters that appears at a specific index
    /// </summary>
    public sealed class GetLetterAtIndexProcessFactory : SimpleRunnableProcessFactory<GetLetterAtIndex, string>
    {
        private GetLetterAtIndexProcessFactory() { }

        public static SimpleRunnableProcessFactory<GetLetterAtIndex, string> Instance { get; } = new GetLetterAtIndexProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"Get character at index '[{nameof(GetLetterAtIndex.Index)}]' in '[{nameof(GetLetterAtIndex.String)}]'");
    }
}