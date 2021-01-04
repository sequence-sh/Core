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
    /// Reorder an array.
    /// </summary>
    [Alias("SortArray")]
    public sealed class ArraySort<T> : CompoundStep<Core.Sequence<T>>
    {
        /// <summary>
        /// The array to modify.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<Core.Sequence<T>> Array { get; set; } = null!;

        /// <summary>
        /// Whether to sort in descending order.
        /// </summary>
        [StepProperty(2)]
        [DefaultValueExplanation("False")]
        public IStep<bool> Descending { get; set; } = new BoolConstant(false);

        /// <inheritdoc />
        public override async Task<Result<Core.Sequence<T>, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var array = await Array.Run(stateMonad, cancellationToken);

            if (array.IsFailure) return array.ConvertFailure<Core.Sequence<T>>();

            var descending = await Descending.Run(stateMonad, cancellationToken);

            if (descending.IsFailure) return descending.ConvertFailure<Core.Sequence<T>>();

            var r = array.Value.Sort(descending.Value);

            return r;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ArraySortStepFactory.Instance;
    }


    /// <summary>
    /// Reorder an array.
    /// </summary>
    public sealed class ArraySortStepFactory : GenericStepFactory
    {
        private ArraySortStepFactory() { }
        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArraySortStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArraySort<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new GenericTypeReference(typeof(Core.Sequence<>), new[] { memberTypeReference });

        /// <inheritdoc />
        public override string OutputTypeExplanation => "ArrayList<T>";


        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.TryGetStep(nameof(ArraySort<object>.Array), StepType)
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Bind(x => x.TryGetGenericTypeReference(typeResolver, 0)
                    .MapError(e => e.WithLocation(freezableStepData)))
                .Map(x => x as ITypeReference)
        ;
    }
}