using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Counts the elements in an array.
    /// </summary>
    public sealed class ArrayCount<T> : CompoundStep<int>
    {
        /// <summary>
        /// The array to count.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<T>> Array { get; set; } = null!;

        /// <inheritdoc />
        public override Result<int, IRunErrors> Run(StateMonad stateMonad) => Array.Run(stateMonad).Map(x => x.Count);

        /// <inheritdoc />
        public override IStepFactory StepFactory => ArrayCountStepFactory.Instance;
    }
}