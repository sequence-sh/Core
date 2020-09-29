using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Gets the letters that appears at a specific index
    /// </summary>
    public sealed class GetLetterAtIndex : CompoundStep<string>
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

        /// <inheritdoc />
        public override Result<string, IRunErrors> Run(StateMonad stateMonad)
        {
            var index = Index.Run(stateMonad);
            if (index.IsFailure) return index.ConvertFailure<string>();

            var str = String.Run(stateMonad);
            if (str.IsFailure) return str;

            if (index.Value < 0 || index.Value >= str.Value.Length)
                return new RunError("Index was outside the bounds of the string", Name, null, ErrorCode.IndexOutOfBounds);

            return str.Value[index.Value].ToString();
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => GetLetterAtIndexStepFactory.Instance;
    }
}