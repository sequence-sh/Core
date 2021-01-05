using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Gets the letters that appears at a specific index
    /// </summary>
    public sealed class CharAtIndex : CompoundStep<StringStream>
    {
        /// <summary>
        /// The string to extract a substring from.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<StringStream> String { get; set; } = null!;


        /// <summary>
        /// The index.
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<int> Index { get; set; } = null!;

        /// <inheritdoc />
        protected override async Task<Result<StringStream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var index = await Index.Run(stateMonad, cancellationToken);
            if (index.IsFailure) return index.ConvertFailure<StringStream>();

            var stringStreamResult = await String.Run(stateMonad, cancellationToken);
            if (stringStreamResult.IsFailure) return stringStreamResult;


            var str = await stringStreamResult.Value.GetStringAsync();

            if (index.Value < 0 || index.Value >= str.Length)
                return new SingleError("Index was outside the bounds of the string", ErrorCode.IndexOutOfBounds, new StepErrorLocation(this));

            var character = str[index.Value].ToString();

            return new StringStream(character);
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => CharAtIndexStepFactory.Instance;
    }

    /// <summary>
    /// Gets the letters that appears at a specific index
    /// </summary>
    public sealed class CharAtIndexStepFactory : SimpleStepFactory<CharAtIndex, StringStream>
    {
        private CharAtIndexStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<CharAtIndex, StringStream> Instance { get; } = new CharAtIndexStepFactory();
    }
}