using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Increment an integer variable by a particular amount
    /// </summary>
    public sealed class IncrementVariable : CompoundStep<Unit>
    {
        /// <summary>
        /// The variable to increment.
        /// </summary>
        [VariableName]
        [Required]
        public VariableName Variable { get; set; }

        /// <summary>
        /// The amount to increment by.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<int> Amount { get; set; } = null!;

        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad) =>
            stateMonad.GetVariable<int>(Variable, Name)
                .Compose(() => Amount.Run(stateMonad))
                .Tap(x => stateMonad.SetVariable(Variable, x.Item1 + x.Item2))
                .Map(x => Unit.Default);

        /// <inheritdoc />
        public override IStepFactory StepFactory => IncrementVariableStepFactory.Instance;
    }
}