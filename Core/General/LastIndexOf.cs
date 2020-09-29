using System;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Gets the last instance of substring in a string.
    /// </summary>
    public sealed class LastIndexOf : CompoundStep<int>
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
        public override Result<int, IRunErrors> Run(StateMonad stateMonad)
        {
            var str = String.Run(stateMonad);
            if (str.IsFailure) return str.ConvertFailure<int>();

            var subString = SubString.Run(stateMonad);
            if (subString.IsFailure) return subString.ConvertFailure<int>();


            return str.Value.LastIndexOf(subString.Value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => LastIndexOfStepFactory.Instance;
    }

    /// <summary>
    /// Gets the last instance of substring in a string.
    /// </summary>
    public sealed class LastIndexOfStepFactory : SimpleStepFactory<LastIndexOf, int>
    {
        private LastIndexOfStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<LastIndexOf, int> Instance { get; } = new LastIndexOfStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Last index of '[{nameof(LastIndexOf.SubString)}]' in '[{nameof(LastIndexOf.String)}]'");
    }
}