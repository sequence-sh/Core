using System;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Gets the first index of an element in an array.
    /// </summary>
    public sealed class FirstIndexOfElementStepFactory : GenericStepFactory
    {
        private FirstIndexOfElementStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new FirstIndexOfElementStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(FirstIndexOfElement<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Int32);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(int));

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableStepData freezableStepData) =>
            freezableStepData.GetArgument(nameof(FirstIndexOfElement<object>.Array))
                .Bind(x => x.TryGetOutputTypeReference())
                .BindCast<ITypeReference, GenericTypeReference>()
                .Map(x => x.ChildTypes)
                .BindSingle();
    }
}