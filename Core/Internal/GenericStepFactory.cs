using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Step factory for generic types.
    /// </summary>
    public abstract class GenericStepFactory : StepFactory
    {
        /// <inheritdoc />
        public override Result<ITypeReference, IError> TryGetOutputTypeReference(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            GetMemberType(freezableStepData, typeResolver)
                .Map(GetOutputTypeReference);

        /// <summary>
        /// Gets the output type from the member type.
        /// </summary>
        protected abstract ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference);


        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableList<Type>.Empty;

        /// <inheritdoc />
        protected override Result<ICompoundStep, IError> TryCreateInstance(StepContext stepContext,
            FreezableStepData freezeData) =>
            GetMemberType(freezeData, stepContext.TypeResolver)
                .Bind(x=> stepContext.TryGetTypeFromReference(x).MapError(e=>e.WithLocation(this, freezeData)))
                .Bind(x => TryCreateGeneric(StepType, x).MapError(e=> e.WithLocation(this, freezeData)));

        /// <summary>
        /// Gets the type
        /// </summary>
        protected abstract Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver);
    }
}