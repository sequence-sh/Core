using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{

    /// <summary>
    /// Negation of a boolean value.
    /// </summary>
    public sealed class Not : CompoundStep<bool>
    {
        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(StateMonad stateMonad) => Boolean.Run(stateMonad).Map(x => !x);

        /// <inheritdoc />
        public override IStepFactory StepFactory => NotStepFactory.Instance;

        /// <summary>
        /// The value to negate.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<bool> Boolean { get; set; } = null!;
    }
}