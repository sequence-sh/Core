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
    /// Calculates the length of the string.
    /// </summary>
    public sealed class StringLength : CompoundStep<int>
    {
        /// <summary>
        /// The string to measure the length of.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> String { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<int, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var str = await String.Run(stateMonad, cancellationToken);
            if (str.IsFailure) return str.ConvertFailure<int>();

            return str.Value.Length;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => StringLengthStepFactory.Instance;
    }

    /// <summary>
    /// Calculates the length of the string.
    /// </summary>
    public sealed class StringLengthStepFactory : SimpleStepFactory<StringLength, int>
    {
        private StringLengthStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<StringLength, int> Instance { get; } = new StringLengthStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Length of [{nameof(StringLength.String)}]");

    }
}