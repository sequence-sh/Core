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
using Reductech.EDR.Core.Util;
using Entity = Reductech.EDR.Core.Entities.Entity;

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
        [StepProperty]
        [Required]
        public IStep<EntityStream> EntityStream { get; set; } = null!;

        /// <summary>
        /// The action to perform repeatedly.
        /// Use the Variable &lt;Entity&gt; to access the entity.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<Unit> Action { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var entities = await EntityStream.Run(stateMonad, cancellationToken);
            if (entities.IsFailure) return entities.ConvertFailure<Unit>();

            var currentState = stateMonad.GetState().ToImmutableDictionary();

            async Task RunAction(Entity record)
            {
                var scopedMonad = new ScopedStateMonad(stateMonad, currentState,
                    new KeyValuePair<VariableName, object>(VariableName.Entity, record));


                var result = await Action.Run(scopedMonad, cancellationToken);

                if (result.IsFailure)
                    throw new ErrorException(result.Error);
            }

            var r = await entities.Value.Act(RunAction);

            return r;
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

        /// <inheritdoc />
        public override IEnumerable<(VariableName VariableName, ITypeReference typeReference)> FixedVariablesSet
        {
            get
            {
                yield return (VariableName.Entity, new ActualTypeReference(typeof(Entity)));
            }
        }

        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>, IError> GetTypeReferencesSet(VariableName variableName, FreezableStepData freezableStepData, TypeResolver typeResolver) =>
            Maybe<ITypeReference>.From(new ActualTypeReference(typeof(Entity)));
    }

}