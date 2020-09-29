using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A step factory that uses default values for most properties.
    /// </summary>
    public abstract class SimpleStepFactory<TStep, TOutput> : StepFactory where TStep : ICompoundStep, new ()
    {
        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(FreezableStepData freezableStepData) => Result.Success(ActualTypeReference.Create(typeof(TOutput)));

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new DefaultStepNameBuilder(TypeName);

        /// <inheritdoc />
        public override Type StepType => typeof(TStep);


        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableArray<Type>.Empty;

        /// <inheritdoc />
        protected override Result<ICompoundStep> TryCreateInstance(StepContext stepContext, FreezableStepData freezableStepData) => new TStep();

        /// <inheritdoc />
        public override string OutputTypeExplanation => typeof(TOutput).Name;
    }
}