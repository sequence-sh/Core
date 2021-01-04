using System.Collections.Generic;
using System.Collections.Immutable;
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
    /// Filter entities according to a function.
    /// </summary>
    [Alias("FilterEntityStream")]
    public sealed class EntityStreamFilter : CompoundStep<Core.Sequence<Entity>>
    {
        /// <inheritdoc />
        public override async Task<Result<Core.Sequence<Entity>, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var entityStreamResult = await EntityStream.Run(stateMonad, cancellationToken);
            if (entityStreamResult.IsFailure) return entityStreamResult.ConvertFailure<Core.Sequence<Entity>>();

            var currentState = stateMonad.GetState().ToImmutableDictionary();

            async IAsyncEnumerable<Entity> Filter(Entity record)
            {
                using var scopedMonad = new ScopedStateMonad(stateMonad, currentState,
                    new KeyValuePair<VariableName, object>(VariableName.Entity, record));

                var result = await Predicate.Run(scopedMonad, cancellationToken);

                if (result.IsFailure)
                    throw new ErrorException(result.Error);

                if (result.Value)
                    yield return record;
            }

            var newStream = entityStreamResult.Value.SelectMany(Filter);

            return newStream;
        }

        /// <summary>
        /// The entities to filter
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<Core.Sequence<Entity>> EntityStream { get; set; } = null!;

        /// <summary>
        /// A function that determines whether an entity should be included from the variable &lt;Entity&gt;
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<bool> Predicate { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityStreamFilterStepFactory.Instance;
    }

    /// <summary>
    /// Filter entities according to a function.
    /// </summary>
    public sealed class EntityStreamFilterStepFactory : SimpleStepFactory<EntityStreamFilter, Core.Sequence<Entity>>
    {
        private EntityStreamFilterStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityStreamFilter, Core.Sequence<Entity>> Instance { get; } = new EntityStreamFilterStepFactory();
    }
}