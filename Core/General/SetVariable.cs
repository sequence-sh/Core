using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{

    /// <summary>
    /// Gets the value of a named variable.
    /// </summary>
    public sealed class SetVariable<T> : CompoundStep<Unit>
    {

        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad) =>
            Value.Run(stateMonad)
                .Bind(x => stateMonad.SetVariable(VariableName, x));

        /// <inheritdoc />
        public override IStepFactory StepFactory => SetVariableStepFactory.Instance;

        /// <summary>
        /// The name of the variable to set.
        /// </summary>
        [VariableName]
        [Required]
        public VariableName VariableName { get; set; }

        /// <summary>
        /// The value to set the variable to.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> Value { get; set; } = null!;
    }
}