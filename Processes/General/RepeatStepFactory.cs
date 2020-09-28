using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Creates an array by repeating an element.
    /// </summary>
    public sealed class RepeatStepFactory : GenericStepFactory
    {
        private RepeatStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new RepeatStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(Repeat<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "List<T>";

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new GenericTypeReference(typeof(List<>), new []{memberTypeReference});

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableStepData freezableStepData) =>
            freezableStepData.GetArgument(nameof(Repeat<object>.Element))
                .Bind(x => x.TryGetOutputTypeReference());
    }
}