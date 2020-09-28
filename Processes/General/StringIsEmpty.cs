using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Returns whether a string is empty.
    /// </summary>
    public sealed class StringIsEmpty : CompoundStep<bool>
    {
        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(StateMonad stateMonad)
        {
            var str = String.Run(stateMonad);
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
}