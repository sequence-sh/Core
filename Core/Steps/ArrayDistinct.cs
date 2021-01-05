using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Removes duplicate entities.
    /// </summary>
    [Alias("Distinct")]
    public sealed class ArrayDistinct<T> : CompoundStep<Array<T>>
    {
        /// <inheritdoc />
        protected override async Task<Result<Array<T>, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var entityStreamResult = await Array.Run(stateMonad, cancellationToken);
            if (entityStreamResult.IsFailure) return entityStreamResult.ConvertFailure<Array<T>>();

            var ignoreCaseResult = await IgnoreCase.Run(stateMonad, cancellationToken);
            if (ignoreCaseResult.IsFailure) return ignoreCaseResult.ConvertFailure<Array<T>>();

            IEqualityComparer<string> comparer = ignoreCaseResult.Value
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

            HashSet<string> usedKeys = new(comparer);

            var currentState = stateMonad.GetState().ToImmutableDictionary();

            async IAsyncEnumerable<T> Filter(T element)
            {
                using var scopedMonad = new ScopedStateMonad(stateMonad, currentState,
                    new KeyValuePair<VariableName, object>(Variable, element!));

                var result = await KeySelector.Run(scopedMonad, cancellationToken)
                    .Map(async x=> await x.GetStringAsync());

                if (result.IsFailure)
                    throw new ErrorException(result.Error);

                if (usedKeys.Add(result.Value))
                    yield return element;
            }

            var newStream = entityStreamResult.Value.SelectMany(Filter);

            return newStream;
        }

        /// <summary>
        /// The array to sort
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<Array<T>> Array { get; set; } = null!;

        /// <summary>
        /// A function that gets the key to distinct by from the variable
        /// To distinct by multiple properties, concatenate several keys
        /// </summary>
        [StepProperty(2)]
        [ScopedFunction]
        [Required]
        public IStep<StringStream> KeySelector { get; set; } = null!;

        /// <summary>
        /// Whether to ignore case when comparing strings.
        /// </summary>
        [StepProperty(3)]
        [DefaultValueExplanation("False")]
        public IStep<bool> IgnoreCase { get; set; } = new BoolConstant(false);

        /// <summary>
        /// The variable name to use in the predicate.
        /// </summary>
        [VariableName(4)]
        [DefaultValueExplanation("<Entity>")]
        public VariableName Variable { get; set; } = VariableName.Entity;

        /// <inheritdoc />
        public override IStepFactory StepFactory => ArrayDistinctStepFactory.Instance;

        /// <inheritdoc />
        public override Result<StepContext, IError> TryGetScopedContext(StepContext baseContext, IFreezableStep scopedStep)
        {
            return baseContext.TryCloneWithScopedStep(Variable, new ActualTypeReference(typeof(T)), scopedStep,
                new StepErrorLocation(this));
        }
    }

    /// <summary>
    /// Removes duplicate entities.
    /// </summary>
    public sealed class ArrayDistinctStepFactory : GenericStepFactory
    {
        private ArrayDistinctStepFactory() {}

        /// <summary>
        /// The Instance
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayDistinctStepFactory();

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new GenericTypeReference(typeof(Array<>), new[] { memberTypeReference });

        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.TryGetStep(nameof(ArrayFilter<object>.Array), StepType)
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Bind(x => x.TryGetGenericTypeReference(typeResolver, 0)
                .MapError(e => e.WithLocation(freezableStepData)))
                .Map(x => x as ITypeReference);

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayDistinct<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array<T>";
    }
}