using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Increment an integer variable by a particular amount
    /// </summary>
    public sealed class IncrementVariableStepFactory :SimpleStepFactory<IncrementVariable, int>
    {
        private IncrementVariableStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<IncrementVariable, int> Instance { get; } = new IncrementVariableStepFactory();

        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName, FreezableStepData freezableStepData) => Maybe<ITypeReference>.From(new ActualTypeReference(typeof(int)));
    }
}