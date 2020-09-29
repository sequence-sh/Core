using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{

    /// <summary>
    /// Executes a statement if a condition is true.
    /// </summary>
    public sealed class Conditional : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad)
        {
            var result = Condition.Run(stateMonad)
                .Bind(r =>
                {
                    if (r)
                        return ThenStep.Run(stateMonad);
                    return ElseStep?.Run(stateMonad) ?? Unit.Default;
                });

            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ConditionalStepFactory.Instance;

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
        public IStep<Unit> ThenStep { get; set; } = null!;

        //TODO else if

        /// <summary>
        /// The Else branch, if it exists.
        /// </summary>
        [StepProperty]
        public IStep<Unit>? ElseStep { get; set; } = null;

    }
}
