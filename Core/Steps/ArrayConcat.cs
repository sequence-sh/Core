using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Concatenates streams of entities
    /// </summary>
    public sealed class ArrayConcat<T> : CompoundStep<Array<T>>
    {
        /// <inheritdoc />
        protected override async Task<Result<Array<T>, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var streamsResult = await Arrays.Run(stateMonad, cancellationToken);
            if (streamsResult.IsFailure) return streamsResult.ConvertFailure<Array<T>>();

            var result =
                streamsResult.Value.SelectMany(al =>
            {
                var asyncEnumerable = al.
                Option.IsT0 ? al.Option.AsT0.ToAsyncEnumerable() : al.Option.AsT1;
                return asyncEnumerable;
            });

            return result;
        }

        /// <summary>
        /// The arrays to concatenate
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<Array<Array<T>>> Arrays { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => ArrayConcatStepFactory.Instance;
    }

    /// <summary>
    /// Concatenates streams of entities
    /// </summary>
    public sealed class ArrayConcatStepFactory : GenericStepFactory
    {
        private ArrayConcatStepFactory() { }

        /// <summary>
        /// The Instance
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayConcatStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayConcat<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new GenericTypeReference(typeof(Array<>), new[] { memberTypeReference });

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array<T>";


        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.TryGetStep(nameof(ArrayConcat<object>.Arrays), StepType)
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Bind(x => x.TryGetGenericTypeReference(typeResolver, 0)
                    .MapError(e => e.WithLocation(freezableStepData)))
                .Bind(x => x.TryGetGenericTypeReference(typeResolver, 0)
                    .MapError(e => e.WithLocation(freezableStepData)))
                .Map(x => x as ITypeReference);
    }

}