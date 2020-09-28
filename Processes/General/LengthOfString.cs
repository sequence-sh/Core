using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
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
        public override Result<int, IRunErrors> Run(StateMonad stateMonad)
        {
            var str = String.Run(stateMonad);
            if (str.IsFailure) return str.ConvertFailure<int>();

            return str.Value.Length;

        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => LengthOfStringStepFactory.Instance;
    }
}