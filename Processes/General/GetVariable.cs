using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{

    /// <summary>
    /// Gets the value of a named variable.
    /// </summary>
    public sealed class GetVariable<T> : CompoundStep<T>
    {
        /// <inheritdoc />
        public override Result<T, IRunErrors> Run(StateMonad stateMonad) => stateMonad.GetVariable<T>(VariableName, Name);

        /// <inheritdoc />
        public override IStepFactory StepFactory => GetVariableStepFactory.Instance;

        /// <summary>
        /// The name of the variable to get.
        /// </summary>
        [VariableName] [Required]
        public VariableName VariableName { get; set; }
    }
}