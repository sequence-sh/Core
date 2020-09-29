using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Reorder an array.
    /// </summary>

    public sealed class SortArrayStepFactory : GenericStepFactory
    {
        private SortArrayStepFactory() { }
        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new SortArrayStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(SortArray<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new GenericTypeReference(typeof(List<>), new []{memberTypeReference});

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] {typeof(SortOrder)};

        /// <inheritdoc />
        public override string OutputTypeExplanation => "List<T>";


        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableStepData freezableStepData) =>
            freezableStepData.GetArgument(nameof(SortArray<object>.Array))
                .Bind(x => x.TryGetOutputTypeReference())
                .BindCast<ITypeReference, GenericTypeReference>()
                .Map(x => x.ChildTypes)
                .BindSingle();
    }
}