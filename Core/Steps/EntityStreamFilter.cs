using System.Collections.Generic;
using System.Collections.Immutable;
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
    /// Filter entities according to a function.
    /// </summary>
    public sealed class EntityStreamFilter : CompoundStep<IAsyncEnumerable<Entity>>
    {
        /// <inheritdoc />
        public override async Task<Result<IAsyncEnumerable<Entity>, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var entityStreamResult = await EntityStream.Run(stateMonad, cancellationToken);
            if (entityStreamResult.IsFailure) return entityStreamResult.ConvertFailure<IAsyncEnumerable<Entity>>();

            var currentState = stateMonad.GetState().ToImmutableDictionary();

            async IAsyncEnumerable<Entity> Filter(Entity record)
            {
                var scopedMonad = new ScopedStateMonad(stateMonad, currentState,
                    new KeyValuePair<VariableName, object>(VariableName.Entity, record));

                var result = await Predicate.Run(scopedMonad, cancellationToken);

                if (result.IsFailure)
                    throw new ErrorException(result.Error);

                if (result.Value)
                    yield return record;
            }

            var newStream = entityStreamResult.Value.SelectMany(Filter);

            return Result.Success<IAsyncEnumerable<Entity>, IError>(newStream);
        }

        /// <summary>
        /// The entities to filter
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<IAsyncEnumerable<Entity>> EntityStream { get; set; } = null!;

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
    public sealed class EntityStreamFilterStepFactory : SimpleStepFactory<EntityStreamFilter, IAsyncEnumerable<Entity>>
    {
        private EntityStreamFilterStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityStreamFilter, IAsyncEnumerable<Entity>> Instance { get; } = new EntityStreamFilterStepFactory();
    }
}