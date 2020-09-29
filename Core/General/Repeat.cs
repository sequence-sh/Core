using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Creates an array by repeating an element.
    /// </summary>
    public sealed class Repeat<T> : CompoundStep<List<T>>
    {
        /// <summary>
        /// The element to repeat.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> Element { get; set; } = null!;

        /// <summary>
        /// The number of times to repeat the element
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<int> Number { get; set; } = null!;

        /// <inheritdoc />
        public override Result<List<T>, IRunErrors> Run(StateMonad stateMonad) =>
            Element.Run(stateMonad).Compose(() => Number.Run(stateMonad))
                .Map(x => Enumerable.Repeat(x.Item1, x.Item2).ToList());

        /// <inheritdoc />
        public override IStepFactory StepFactory => RepeatStepFactory.Instance;
    }
}