using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// A step factory that uses default values for most properties.
    /// </summary>
    public abstract class SimpleStepFactory<TProcess, TOutput> : StepFactory where TProcess : ICompoundStep, new ()
    {
        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(FreezableStepData freezableStepData) => Result.Success(ActualTypeReference.Create(typeof(TOutput)));

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new DefaultStepNameBuilder(TypeName);

        /// <inheritdoc />
        public override Type StepType => typeof(TProcess);


        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableArray<Type>.Empty;

        /// <inheritdoc />
        protected override Result<ICompoundStep> TryCreateInstance(StepContext stepContext, FreezableStepData freezableStepData) => new TProcess();

        /// <inheritdoc />
        public override string OutputTypeExplanation => typeof(TOutput).Name;
    }
}