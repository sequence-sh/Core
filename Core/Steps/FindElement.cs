using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Gets the first index of an element in an array.
    /// Returns -1 if the element is not present.
    /// </summary>
    public sealed class FindElement<T> : CompoundStep<int>
    {
        /// <summary>
        /// The array to check.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<List<T>> Array { get; set; } = null!;

        /// <summary>
        /// The element to look for.
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<T> Element { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<int, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var arrayResult = await Array.Run(stateMonad, cancellationToken);

            if (arrayResult.IsFailure) return arrayResult.ConvertFailure<int>();

            var elementResult = await Element.Run(stateMonad, cancellationToken);

            if (elementResult.IsFailure) return elementResult.ConvertFailure<int>();

            var r = arrayResult.Value.IndexOf(elementResult.Value);

            return r;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => FindElementStepFactory.Instance;
    }

    /// <summary>
    /// Gets the first index of an element in an array.
    /// </summary>
    public sealed class FindElementStepFactory : GenericStepFactory
    {
        private FindElementStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new FindElementStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(FindElement<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Int32);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(int));

        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData, TypeResolver typeResolver) =>
            freezableStepData.TryGetStep(nameof(FindElement<object>.Array), StepType)
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Bind(x=>x.TryGetGenericTypeReference(typeResolver, 0)
                .MapError(e=>e.WithLocation(freezableStepData))
                )
                .Map(x=> x as ITypeReference);
    }
}