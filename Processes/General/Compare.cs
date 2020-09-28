using System;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Compares two items.
    /// </summary>
    public sealed class Compare<T> : CompoundStep<bool> where T : IComparable
    {
        /// <summary>
        /// The item to the left of the operator.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> Left { get; set; } = null!;

        /// <summary>
        /// The operator to use for comparison.
        /// </summary>
        [StepProperty]
        [Required]

        public IStep<CompareOperator> Operator { get; set; } = null!;

        /// <summary>
        /// The item to the right of the operator.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> Right { get; set; } = null!;


        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(StateMonad stateMonad)
        {
            var result = Left.Run(stateMonad)
                .Compose(() => Operator.Run(stateMonad), () => Right.Run(stateMonad))
                .Bind(x => CompareItems(x.Item1, x.Item2, x.Item3));


            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => CompareStepFactory.Instance;

        private static Result<bool, IRunErrors> CompareItems(T item1, CompareOperator compareOperator, T item2)
        {
            return compareOperator switch
            {
                CompareOperator.Equals => item1.Equals(item2),
                CompareOperator.NotEquals => !item1.Equals(item2),
                CompareOperator.LessThan => item1.CompareTo(item2) < 0,
                CompareOperator.LessThanOrEqual => item1.CompareTo(item2) <= 0,
                CompareOperator.GreaterThan => item1.CompareTo(item2) > 0,
                CompareOperator.GreaterThanOrEqual => item1.CompareTo(item2) >= 0,
                _ => throw new ArgumentOutOfRangeException(nameof(compareOperator), compareOperator, null)
            };
        }

    }
}