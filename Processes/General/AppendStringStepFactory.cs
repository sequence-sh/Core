using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
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