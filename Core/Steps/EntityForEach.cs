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
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Perform an action on each entity in the stream.
    /// </summary>
    public sealed class EntityForEach : CompoundStep<Unit>
    {
        /// <summary>
        /// The entities to iterate over.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<IAsyncEnumerable<Entity>> EntityStream { get; set; } = null!;

        /// <summary>
        /// The action to perform repeatedly.
        /// Use the Variable &lt;Entity&gt; to access the entity.
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<Unit> Action { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var entities = await EntityStream.Run(stateMonad, cancellationToken);
            if (entities.IsFailure) return entities.ConvertFailure<Unit>();

            var currentState = stateMonad.GetState().ToImmutableDictionary();
            var errors = new List<IError>();

            await foreach (var entity in entities.Value.WithCancellation(cancellationToken))
            {
                var scopedMonad = new ScopedStateMonad(stateMonad, currentState,
                    new KeyValuePair<VariableName, object>(VariableName.Entity, entity));


                var result = await Action.Run(scopedMonad, cancellationToken);

                if (result.IsFailure)
                    errors.Add(result.Error);
            }

            if(errors.Any())
                return Result.Failure<Unit, IError>( ErrorList.Combine(errors));

            return Unit.Default;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityForEachStepFactory.Instance;
    }


    /// <summary>
    /// Perform an action on each record in the stream.
    /// </summary>
    public sealed class EntityForEachStepFactory : SimpleStepFactory<EntityForEach, Unit>
    {
        private EntityForEachStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityForEach, Unit> Instance { get; } = new EntityForEachStepFactory();
    }

}