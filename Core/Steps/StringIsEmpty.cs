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
    /// Returns whether a string is empty.
    /// </summary>
    public sealed class StringIsEmpty : CompoundStep<bool>
    {
        /// <inheritdoc />
        public override async Task<Result<bool, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var str = await String.Run(stateMonad, cancellationToken);
            if (str.IsFailure) return str.ConvertFailure<bool>();

            return string.IsNullOrWhiteSpace(str.Value);
        }

        /// <summary>
        /// The string to check for being empty.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> String { get; set; } = null!;


        /// <inheritdoc />
        public override IStepFactory StepFactory => StringIsEmptyStepFactory.Instance;
    }

    /// <summary>
    /// Returns whether a string is empty.
    /// </summary>
    public sealed class StringIsEmptyStepFactory : SimpleStepFactory<StringIsEmpty, bool>
    {
        private StringIsEmptyStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<StringIsEmpty, bool> Instance { get; } = new StringIsEmptyStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"'[{nameof(LengthOfString.String)}]' is empty?");
    }
}