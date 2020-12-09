using System;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A step factory that uses default values for most properties.
    /// </summary>
    public abstract class SimpleStepFactory<TStep, TOutput> : StepFactory where TStep : ICompoundStep<TOutput>, new ()
    {
        /// <inheritdoc />
        public override Result<ITypeReference, IError> TryGetOutputTypeReference(FreezableStepData freezableStepData,
            TypeResolver typeResolver) => Result.Success<ITypeReference, IError>(ActualTypeReference.Create(typeof(TOutput)));

        /// <inheritdoc />
        public override Type StepType => typeof(TStep);

        /// <inheritdoc />
        protected override Result<ICompoundStep, IError> TryCreateInstance(StepContext stepContext,
            FreezableStepData freezeData) => new TStep();

        /// <inheritdoc />
        public override string OutputTypeExplanation => typeof(TOutput).Name;
    }
}