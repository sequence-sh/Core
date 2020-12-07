using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Filter entities according to a function.
    /// </summary>
    public sealed class EntityStreamFilter : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task<Result<EntityStream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var entityStreamResult = await EntityStream.Run(stateMonad, cancellationToken);
            if (entityStreamResult.IsFailure) return entityStreamResult.ConvertFailure<EntityStream>();

            var currentState = stateMonad.GetState().ToImmutableDictionary();

            async Task<Maybe<Entity>> FilterAction(Entity record)
            {
                var scopedMonad = new ScopedStateMonad(stateMonad, currentState,
                    new KeyValuePair<VariableName, object>(VariableName.Entity, record));

                var result = await Predicate.Run(scopedMonad, cancellationToken);

                if (result.IsFailure)
                    throw new ErrorException(result.Error);

                if(result.Value)
                    return Maybe<Entity>.From(record);
                return Maybe<Entity>.None;
            }

            var newStream = entityStreamResult.Value.ApplyMaybe(FilterAction);

            return newStream;
        }

        /// <summary>
        /// The entities to filter
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<EntityStream> EntityStream { get; set; } = null!;

        /// <summary>
        /// A function that determines whether an entity should be included from the variable &lt;Entity&gt;
        /// </summary>
        [StepProperty(Order = 2)]
        [Required]
        public IStep<bool> Predicate { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityStreamFilterStepFactory.Instance;
    }

    /// <summary>
    /// Filter entities according to a function.
    /// </summary>
    public sealed class EntityStreamFilterStepFactory : SimpleStepFactory<EntityStreamFilter, EntityStream>
    {
        private EntityStreamFilterStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityStreamFilter, EntityStream> Instance { get; } = new EntityStreamFilterStepFactory();
    }
}