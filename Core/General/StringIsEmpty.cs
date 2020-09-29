using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
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