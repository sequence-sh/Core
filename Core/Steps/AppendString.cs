using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Appends a string to an existing string variable.
    /// </summary>
    public sealed class AppendString : CompoundStep<Unit>
    {
        /// <summary>
        /// The variable to append to.
        /// </summary>
        [VariableName(Order = 1)]
        [Required]
        public VariableName Variable { get; set; }


        /// <summary>
        /// The string to append.
        /// </summary>
        [StepProperty(Order = 2)]
        [Required]
        public IStep<string> String { get; set; } = null!;

        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad)
        {
            var currentValue = stateMonad.GetVariable<string>(Variable, Name);
            if (currentValue.IsFailure)
                return currentValue.ConvertFailure<Unit>();


            var str = String.Run(stateMonad);
            if (str.IsFailure)
                return str.ConvertFailure<Unit>();

            var value = currentValue.Value + str.Value;

            var r = stateMonad.SetVariable(Variable, value);
            if (r.IsFailure)
                return r.ConvertFailure<Unit>();

            return Unit.Default;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => AppendStringStepFactory.Instance;
    }

    /// <summary>
    /// Appends a string to an existing string variable.
    /// </summary>
    public sealed class AppendStringStepFactory : SimpleStepFactory<AppendString, Unit>
    {
        private AppendStringStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<AppendString, Unit> Instance { get; } = new AppendStringStepFactory();

        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName, FreezableStepData freezableStepData) => Maybe<ITypeReference>.From(new ActualTypeReference(typeof(string)));


        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Append [{nameof(AppendString.String)}] to [{nameof(AppendString.Variable)}]");



    }
}