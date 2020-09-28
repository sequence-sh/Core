using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Gets a substring from a string.
    /// </summary>
    public sealed class GetSubstring : CompoundStep<string>
    {
        /// <summary>
        /// The string to extract a substring from.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> String { get; set; } = null!;

        /// <summary>
        /// The index.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<int> Index { get; set; } = null!;

        /// <summary>
        /// The length of the substring to extract.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<int> Length { get; set; } = null!;


        /// <inheritdoc />
        public override Result<string, IRunErrors> Run(StateMonad stateMonad)
        {
            var str = String.Run(stateMonad);
            if (str.IsFailure) return str;
            var index = Index.Run(stateMonad);
            if (index.IsFailure) return index.ConvertFailure<string>();
            var length = Length.Run(stateMonad);
            if (length.IsFailure) return length.ConvertFailure<string>();


            if (index.Value < 0 || index.Value >= str.Value.Length)
                return new RunError("Index was outside the bounds of the string", Name, null, ErrorCode.IndexOutOfBounds);

            return str.Value.Substring(index.Value, length.Value);
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => GetSubstringStepFactory.Instance;
    }
}