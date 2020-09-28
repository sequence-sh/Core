using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Checks if an array is empty.
    /// </summary>
    public sealed class ArrayIsEmpty<T> : CompoundStep<bool>
    {
        /// <summary>
        /// The array to check for emptiness.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<T>> Array { get; set; } = null!;

        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(StateMonad stateMonad) => Array.Run(stateMonad).Map(x => !x.Any());

        /// <inheritdoc />
        public override IStepFactory StepFactory => ArrayIsEmptyStepFactory.Instance;
    }
}