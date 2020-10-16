using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Calculates the length of the string.
    /// </summary>
    public sealed class LengthOfString : CompoundStep<int>
    {
        /// <summary>
        /// The string to measure the length of.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> String { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<int, IRunErrors>>  Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var str = await String.Run(stateMonad, cancellationToken);
            if (str.IsFailure) return str.ConvertFailure<int>();

            return str.Value.Length;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => LengthOfStringStepFactory.Instance;
    }

    /// <summary>
    /// Calculates the length of the string.
    /// </summary>
    public sealed class LengthOfStringStepFactory : SimpleStepFactory<LengthOfString, int>
    {
        private LengthOfStringStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<LengthOfString, int> Instance { get; } = new LengthOfStringStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Length of [{nameof(LengthOfString.String)}]");

    }
}