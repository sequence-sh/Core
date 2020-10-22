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
        public override async Task<Result<string, IError>>  Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var str = await String.Run(stateMonad, cancellationToken);
            if (str.IsFailure) return str;
            var index = await Index.Run(stateMonad, cancellationToken);
            if (index.IsFailure) return index.ConvertFailure<string>();
            var length = await Length.Run(stateMonad, cancellationToken);
            if (length.IsFailure) return length.ConvertFailure<string>();


            if (index.Value < 0 || index.Value >= str.Value.Length)
                return new SingleError("Index was outside the bounds of the string", ErrorCode.IndexOutOfBounds, new StepErrorLocation(this));

            return str.Value.Substring(index.Value, length.Value);
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => GetSubstringStepFactory.Instance;
    }

    /// <summary>
    /// Gets a substring from a string.
    /// </summary>
    public sealed class GetSubstringStepFactory : SimpleStepFactory<GetSubstring, string>
    {
        private GetSubstringStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<GetSubstring, string> Instance { get; } = new GetSubstringStepFactory();
    }
}