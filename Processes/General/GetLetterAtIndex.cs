using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Gets the letters that appears at a specific index
    /// </summary>
    public sealed class GetLetterAtIndex : CompoundRunnableProcess<string>
    {
        /// <summary>
        /// The string to extract a substring from.
        /// </summary>
        [RunnableProcessPropertyAttribute]
        [Required]
        public IRunnableProcess<string> String { get; set; } = null!;


        /// <summary>
        /// The index.
        /// </summary>
        [RunnableProcessPropertyAttribute]
        [Required]
        public IRunnableProcess<int> Index { get; set; } = null!;

        /// <inheritdoc />
        public override Result<string, IRunErrors> Run(ProcessState processState)
        {
            var index = Index.Run(processState);
            if (index.IsFailure) return index.ConvertFailure<string>();

            var str = String.Run(processState);
            if (str.IsFailure) return str;

            if (index.Value < 0 || index.Value >= str.Value.Length)
                return new RunError("Index was outside the bounds of the string", Name, null, ErrorCode.IndexOutOfBounds);

            return str.Value[index.Value].ToString();
        }

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => GetLetterAtIndexProcessFactory.Instance;
    }

    /// <summary>
    /// Gets the letters that appears at a specific index
    /// </summary>
    public sealed class GetLetterAtIndexProcessFactory : SimpleRunnableProcessFactory<GetLetterAtIndex, string>
    {
        private GetLetterAtIndexProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleRunnableProcessFactory<GetLetterAtIndex, string> Instance { get; } = new GetLetterAtIndexProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"Get character at index '[{nameof(GetLetterAtIndex.Index)}]' in '[{nameof(GetLetterAtIndex.String)}]'");
    }
}