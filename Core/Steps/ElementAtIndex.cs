using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
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
        [StepProperty(1)]
        [Required]
        public IStep<List<T>> Array { get; set; } = null!;

        /// <summary>
        /// The index to get the element at.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<int> Index { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<T, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            return await Array.Run(stateMonad, cancellationToken)
                .Compose(() => Index.Run(stateMonad, cancellationToken))
                .Ensure(x => x.Item2 >= 0 && x.Item2 < x.Item1.Count,
                    new SingleError("Index was out of the range of the array.", ErrorCode.IndexOutOfBounds, new StepErrorLocation(this)))
                .Map(x => x.Item1[x.Item2]);
        }

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
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.GetStep(nameof(ElementAtIndex<object>.Array), TypeName)

                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Bind(x=>x.TryGetGenericTypeReference(typeResolver, 0)
                .MapError(e=>e.WithLocation(this, freezableStepData))
                )
                .Map(x=>x as ITypeReference);
    }
}