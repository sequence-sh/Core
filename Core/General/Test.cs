using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Returns one result if a condition is true and another if the condition is false.
    /// </summary>
    public sealed class Test<T> : CompoundStep<T>
    {
        /// <inheritdoc />
        public override Result<T, IRunErrors> Run(StateMonad stateMonad)
        {
            var result = Condition.Run(stateMonad)
                .Bind(r => r ? ThenValue.Run(stateMonad) : ElseValue.Run(stateMonad));

            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => TestStepFactory.Instance;


        /// <summary>
        /// Whether to follow the Then Branch
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<bool> Condition { get; set; } = null!;

        /// <summary>
        /// The Then Branch.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> ThenValue { get; set; } = null!;

        /// <summary>
        /// The Else branch, if it exists.
        /// </summary>
        [StepProperty]
        public IStep<T> ElseValue { get; set; } = null!;
    }
}