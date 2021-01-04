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
    /// Apply a function to every entity in an entity stream.
    /// </summary>
    public sealed class EntityMap : CompoundStep<Core.Array<Entity>>
    {
        /// <inheritdoc />
        public override async Task<Result<Core.Array<Entity>, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var entityStreamResult = await EntityStream.Run(stateMonad, cancellationToken);
            if (entityStreamResult.IsFailure) return entityStreamResult.ConvertFailure<Core.Array<Entity>>();

            var currentState = stateMonad.GetState().ToImmutableDictionary();

            async ValueTask<Entity> Action(Entity record)
            {
                using var scopedMonad = new ScopedStateMonad(stateMonad, currentState,
                    new KeyValuePair<VariableName, object>(VariableName.Entity, record));

                var result = await Function.Run(scopedMonad, cancellationToken);

                if (result.IsFailure)
                    throw new ErrorException(result.Error);

                return result.Value;
            }

            var newStream = entityStreamResult.Value.SelectAwait(Action);

            return newStream;
        }

        /// <summary>
        /// The entities to sort
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<Core.Array<Entity>> EntityStream { get; set; } = null!;

        /// <summary>
        /// A function to get the mapped entity, using the variable &lt;Entity&gt;
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<Entity> Function { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityMapStepFactory.Instance;
    }

    /// <summary>
    /// Apply a function to every entity in an entity stream.
    /// </summary>
    public sealed class EntityMapStepFactory : SimpleStepFactory<EntityMap, Core.Array<Entity>>
    {
        private EntityMapStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityMap, Core.Array<Entity>> Instance { get; } = new EntityMapStepFactory();
    }
}