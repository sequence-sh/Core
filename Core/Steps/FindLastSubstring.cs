using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Gets the last instance of substring in a string.
    /// Returns -1 if the substring is not present
    /// </summary>
    public sealed class FindLastSubstring : CompoundStep<int>
    {
        /// <summary>
        /// The string to check.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> String { get; set; } = null!;

        /// <summary>
        /// The substring to find.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> SubString { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<int, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var str = await String.Run(stateMonad, cancellationToken);
            if (str.IsFailure) return str.ConvertFailure<int>();

            var subString = await SubString.Run(stateMonad, cancellationToken);
            if (subString.IsFailure) return subString.ConvertFailure<int>();


            return str.Value.LastIndexOf(subString.Value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => FindLastSubstringStepFactory.Instance;
    }

    /// <summary>
    /// Gets the last instance of substring in a string.
    /// </summary>
    public sealed class FindLastSubstringStepFactory : SimpleStepFactory<FindLastSubstring, int>
    {
        private FindLastSubstringStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<FindLastSubstring, int> Instance { get; } = new FindLastSubstringStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Last index of '[{nameof(FindLastSubstring.SubString)}]' in '[{nameof(FindLastSubstring.String)}]'");
    }
}