using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Reorder an array.
    /// </summary>
    public sealed class SortArray<T> : CompoundStep<List<T>>
    {
        /// <summary>
        /// The array to modify.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<T>> Array { get; set; } = null!;

        /// <summary>
        /// The order to use.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<SortOrder> Order { get; set; } = null!;

        /// <inheritdoc />
        public override Result<List<T>, IRunErrors> Run(StateMonad stateMonad) =>
            Array.Run(stateMonad)
                .Compose(() => Order.Run(stateMonad))
                .Map(x => Sort(x.Item1, x.Item2));

        private static List<T> Sort(IEnumerable<T> list, SortOrder sortOrder) =>
            sortOrder switch
            {
                SortOrder.Ascending => list.OrderBy(x => x).ToList(),
                SortOrder.Descending => list.OrderByDescending(x => x).ToList(),
                _ => throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, null)
            };

        /// <inheritdoc />
        public override IStepFactory StepFactory => SortArrayStepFactory.Instance;
    }
}