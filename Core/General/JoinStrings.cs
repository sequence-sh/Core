using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Join strings with a delimiter.
    /// </summary>
    public sealed class JoinStrings : CompoundStep<string>
    {
        /// <summary>
        /// The delimiter to use.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Delimiter { get; set; } = null!;

        /// <summary>
        /// The elements to iterate over.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<string>> List { get; set; } = null!;

        /// <inheritdoc />
        public override Result<string, IRunErrors> Run(StateMonad stateMonad)
        {
            var list = List.Run(stateMonad);
            if (list.IsFailure) return list.ConvertFailure<string>();

            var delimiter = Delimiter.Run(stateMonad);
            if (delimiter.IsFailure) return delimiter;


            var result = string.Join(delimiter.Value, list.Value);

            return result;

        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => JoinStringsStepFactory.Instance;
    }
}