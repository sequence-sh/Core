using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Gets the first index of an element in an array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class FirstIndexOfElement<T> : CompoundStep<int>
    {
        /// <summary>
        /// The array to check.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<T>> Array { get; set; } = null!;

        /// <summary>
        /// The element to look for.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> Element { get; set; } = null!;

        /// <inheritdoc />
        public override Result<int, IRunErrors> Run(StateMonad stateMonad) =>
            Array.Run(stateMonad).Compose(() => Element.Run(stateMonad))
                .Map(x => x.Item1.IndexOf(x.Item2));

        /// <inheritdoc />
        public override IStepFactory StepFactory => FirstIndexOfElementStepFactory.Instance;
    }
}