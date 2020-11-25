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
using Entity = Reductech.EDR.Core.Entities.Entity;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Removes duplicate entities.
    /// </summary>
    public sealed class DistinctEntities : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task<Result<EntityStream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var entityStreamResult = await EntityStream.Run(stateMonad, cancellationToken);
            if (entityStreamResult.IsFailure) return entityStreamResult.ConvertFailure<EntityStream>();

            var caseSensitiveResult = await CaseSensitive.Run(stateMonad, cancellationToken);
            if (caseSensitiveResult.IsFailure) return caseSensitiveResult.ConvertFailure<EntityStream>();

            HashSet<string> usedKeys = new HashSet<string>();
            var currentState = stateMonad.GetState().ToImmutableDictionary();

            async Task<Maybe<Entity>> FilterAction(Entity record)
            {
                var scopedMonad = new ScopedStateMonad(stateMonad, currentState,
                    new KeyValuePair<VariableName, object>(VariableName.Entity, record));

                var result = await DistinctBy.Run(scopedMonad, cancellationToken);

                if (result.IsFailure)
                    throw new ErrorException(result.Error);

                if(usedKeys.Add(result.Value))
                    return Maybe<Entity>.From(record);
                return Maybe<Entity>.None;
            }

            var newStream = entityStreamResult.Value.ApplyMaybe(FilterAction);

            return newStream;
        }

        /// <summary>
        /// The entities to sort
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<EntityStream> EntityStream { get; set; } = null!;

        /// <summary>
        /// A function that gets the key to distinct by from the variable &lt;Entity&gt;
        /// To distinct by multiple properties, concatenate several keys
        /// </summary>
        [StepProperty(Order = 2)]
        [Required]
        public IStep<string> DistinctBy { get; set; } = null!;

        /// <summary>
        /// Whether comparisons should be case sensitive.
        /// </summary>
        [StepProperty(Order = 2)]
        [DefaultValueExplanation("true")]
        public IStep<bool> CaseSensitive { get; set; } = new Constant<bool>(true);
        /// <inheritdoc />
        public override IStepFactory StepFactory => DistinctEntitiesStepFactory.Instance;
    }

    /// <summary>
    /// Removes duplicate entities.
    /// </summary>
    public sealed class DistinctEntitiesStepFactory : SimpleStepFactory<DistinctEntities, EntityStream>
    {
        private DistinctEntitiesStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<DistinctEntities, EntityStream> Instance { get; } = new DistinctEntitiesStepFactory();

        /// <inheritdoc />
        public override IEnumerable<(VariableName VariableName, ITypeReference typeReference)> FixedVariablesSet
        {
            get
            {
                yield return (VariableName.Entity, new ActualTypeReference(typeof(Entity)));
            }
        }
    }
}