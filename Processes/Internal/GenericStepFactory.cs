using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// Step factory for generic types.
    /// </summary>
    public abstract class GenericStepFactory : StepFactory
    {
        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(FreezableStepData freezableStepData) => GetMemberType(freezableStepData).Map(GetOutputTypeReference);

        /// <summary>
        /// Gets the output type from the member type.
        /// </summary>
        protected abstract ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference);

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new DefaultStepNameBuilder(TypeName);

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableList<Type>.Empty;

        /// <inheritdoc />
        protected override Result<ICompoundStep> TryCreateInstance(StepContext stepContext, FreezableStepData freezableStepData) =>
            GetMemberType(freezableStepData)
                .Bind(stepContext.TryGetTypeFromReference)
                .Bind(x => TryCreateGeneric(StepType, x));

        /// <summary>
        /// Gets the type
        /// </summary>
        protected abstract Result<ITypeReference> GetMemberType(FreezableStepData freezableStepData);
    }
}