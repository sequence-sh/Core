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
    public sealed class EntityStreamDistinct : CompoundStep<AsyncList<Entity>>
    {
        /// <inheritdoc />
        public override async Task<Result<AsyncList<Entity>, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var entityStreamResult = await EntityStream.Run(stateMonad, cancellationToken);
            if (entityStreamResult.IsFailure) return entityStreamResult.ConvertFailure<AsyncList<Entity>>();

            var ignoreCaseResult = await IgnoreCase.Run(stateMonad, cancellationToken);
            if (ignoreCaseResult.IsFailure) return ignoreCaseResult.ConvertFailure<AsyncList<Entity>>();

            IEqualityComparer<string> comparer = ignoreCaseResult.Value
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

            HashSet<string> usedKeys = new(comparer);

            var currentState = stateMonad.GetState().ToImmutableDictionary();

            async IAsyncEnumerable<Entity> Filter(Entity record)
            {
                using var scopedMonad = new ScopedStateMonad(stateMonad, currentState,
                    new KeyValuePair<VariableName, object>(VariableName.Entity, record));

                var result = await KeySelector.Run(scopedMonad, cancellationToken)
                    .Map(async x=> await x.GetStringAsync());

                if (result.IsFailure)
                    throw new ErrorException(result.Error);

                if (usedKeys.Add(result.Value))
                    yield return record;
            }

            var newStream = entityStreamResult.Value.SelectMany(Filter);

            return newStream;
        }

        /// <summary>
        /// The entities to sort
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<AsyncList<Entity>> EntityStream { get; set; } = null!;

        /// <summary>
        /// A function that gets the key to distinct by from the variable &lt;Entity&gt;
        /// To distinct by multiple properties, concatenate several keys
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<StringStream> KeySelector { get; set; } = null!;

        /// <summary>
        /// Whether to ignore case when comparing strings.
        /// </summary>
        [StepProperty(3)]
        [DefaultValueExplanation("False")]
        public IStep<bool> IgnoreCase { get; set; } = new BoolConstant(false);
        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityStreamDistinctStepFactory.Instance;
    }

    /// <summary>
    /// Removes duplicate entities.
    /// </summary>
    public sealed class EntityStreamDistinctStepFactory : SimpleStepFactory<EntityStreamDistinct, AsyncList<Entity>>
    {
        private EntityStreamDistinctStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityStreamDistinct, AsyncList<Entity>> Instance { get; } = new EntityStreamDistinctStepFactory();
    }
}