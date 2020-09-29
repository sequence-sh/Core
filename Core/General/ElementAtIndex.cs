using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Gets the array element at a particular index.
    /// </summary>
    public sealed class ElementAtIndex<T> : CompoundStep<T>
    {
        /// <summary>
        /// The array to check.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<T>> Array { get; set; } = null!;

        /// <summary>
        /// The index to get the element at.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<int> Index { get; set; } = null!;

        /// <inheritdoc />
        public override Result<T, IRunErrors> Run(StateMonad stateMonad) =>
            Array.Run(stateMonad)
                .Compose(() => Index.Run(stateMonad))
                .Ensure(x => x.Item2 >= 0 && x.Item2 < x.Item1.Count,
                    new RunError( "Index was out of the range of the array.", Name, null, ErrorCode.IndexOutOfBounds))
                .Map(x=>x.Item1[x.Item2]);

        /// <inheritdoc />
        public override IStepFactory StepFactory => ElementAtIndexStepFactory.Instance;
    }
}