using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Gets the array element at a particular index.
    /// </summary>
    public sealed class ElementAtIndex<T> : CompoundStep<T>
    {
        /// <summary>
        /// The array to check.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<T>> Array { get; set; } = null!;

        /// <summary>
        /// The index to get the element at.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<int> Index { get; set; } = null!;

        /// <inheritdoc />
        public override Result<T, IRunErrors> Run(StateMonad stateMonad) =>
            Array.Run(stateMonad)
                .Compose(() => Index.Run(stateMonad))
                .Ensure(x => x.Item2 >= 0 && x.Item2 < x.Item1.Count,
                    new RunError( "Index was out of the range of the array.", Name, null, ErrorCode.IndexOutOfBounds))
                .Map(x=>x.Item1[x.Item2]);

        /// <inheritdoc />
        public override IStepFactory StepFactory => ElementAtIndexStepFactory.Instance;
    }

    /// <summary>
    /// Gets the array element at a particular index.
    /// </summary>
    public sealed class ElementAtIndexStepFactory : GenericStepFactory
    {
        private ElementAtIndexStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ElementAtIndexStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ElementAtIndex<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.GetArgument(nameof(ElementAtIndex<object>.Array))
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .BindCast<ITypeReference, GenericTypeReference>()
                .Map(x => x.ChildTypes)
                .BindSingle();
    }
}