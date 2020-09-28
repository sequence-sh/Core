using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Represents an ordered collection of objects.
    /// </summary>
    public sealed class Array<T> : CompoundStep<List<T>>
    {
        /// <inheritdoc />
        public override Result<List<T>, IRunErrors> Run(StateMonad stateMonad)
        {
            var result = Elements.Select(x => x.Run(stateMonad))
                .Combine(RunErrorList.Combine)
                .Map(x => x.ToList());

            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ArrayStepFactory.Instance;

        /// <summary>
        /// The elements of this array.
        /// </summary>
        [StepListProperty]
        [Required]
        public IReadOnlyList<IStep<T>> Elements { get; set; } = null!;
    }
}