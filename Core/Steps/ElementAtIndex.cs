using System;
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
    /// Gets the array element at a particular index.
    /// </summary>
    [Alias("FromArray")]
    public sealed class ElementAtIndex<T> : CompoundStep<T>
    {
        /// <summary>
        /// The array to check.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<Core.Array<T>> Array { get; set; } = null!;

        /// <summary>
        /// The index to get the element at.
        /// </summary>
        [StepProperty(2)]
        [Required]
        [Alias("GetElement")]
        public IStep<int> Index { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<T, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var arrayResult = await Array.Run(stateMonad, cancellationToken);

            if (arrayResult.IsFailure) return arrayResult.ConvertFailure<T>();

            var indexResult = await Index.Run(stateMonad, cancellationToken);

            if (indexResult.IsFailure) return indexResult.ConvertFailure<T>();

            var r = await arrayResult.Value.ElementAtAsync(indexResult.Value, new StepErrorLocation(this), cancellationToken);

            return r;
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
            freezableStepData.TryGetStep(nameof(ElementAtIndex<object>.Array), StepType)

                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Bind(x=>x.TryGetGenericTypeReference(typeResolver, 0)
                .MapError(e=>e.WithLocation(freezableStepData))
                )
                .Map(x=>x as ITypeReference);
    }
}