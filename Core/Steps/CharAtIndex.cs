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
    /// Gets the letters that appears at a specific index
    /// </summary>
    public sealed class CharAtIndex : CompoundStep<string>
    {
        /// <summary>
        /// The string to extract a substring from.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<string> String { get; set; } = null!;


        /// <summary>
        /// The index.
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<int> Index { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<string, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var index = await Index.Run(stateMonad, cancellationToken);
            if (index.IsFailure) return index.ConvertFailure<string>();

            var str = await String.Run(stateMonad, cancellationToken);
            if (str.IsFailure) return str;

            if (index.Value < 0 || index.Value >= str.Value.Length)
                return new SingleError("Index was outside the bounds of the string", ErrorCode.IndexOutOfBounds, new StepErrorLocation(this));

            return str.Value[index.Value].ToString();
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => CharAtIndexStepFactory.Instance;
    }

    /// <summary>
    /// Gets the letters that appears at a specific index
    /// </summary>
    public sealed class CharAtIndexStepFactory : SimpleStepFactory<CharAtIndex, string>
    {
        private CharAtIndexStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<CharAtIndex, string> Instance { get; } = new CharAtIndexStepFactory();
    }
}