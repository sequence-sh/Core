using System;
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
    /// Removes duplicate entities.
    /// </summary>
    public sealed class EntityStreamDistinct : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task<Result<EntityStream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var entityStreamResult = await EntityStream.Run(stateMonad, cancellationToken);
            if (entityStreamResult.IsFailure) return entityStreamResult.ConvertFailure<EntityStream>();

            var ignoreCaseResult = await IgnoreCase.Run(stateMonad, cancellationToken);
            if (ignoreCaseResult.IsFailure) return ignoreCaseResult.ConvertFailure<EntityStream>();

            IEqualityComparer<string> comparer = ignoreCaseResult.Value
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

            HashSet<string> usedKeys = new HashSet<string>(comparer);

            var currentState = stateMonad.GetState().ToImmutableDictionary();

            async Task<Maybe<Entity>> FilterAction(Entity record)
            {
                var scopedMonad = new ScopedStateMonad(stateMonad, currentState,
                    new KeyValuePair<VariableName, object>(VariableName.Entity, record));

                var result = await KeySelector.Run(scopedMonad, cancellationToken);

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
        public IStep<string> KeySelector { get; set; } = null!;

        /// <summary>
        /// Whether to ignore case when comparing strings.
        /// </summary>
        [StepProperty(Order = 3)]
        [DefaultValueExplanation("False")]
        public IStep<bool> IgnoreCase { get; set; } = new Constant<bool>(false);
        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityStreamDistinctStepFactory.Instance;
    }

    /// <summary>
    /// Removes duplicate entities.
    /// </summary>
    public sealed class EntityStreamDistinctStepFactory : SimpleStepFactory<EntityStreamDistinct, EntityStream>
    {
        private EntityStreamDistinctStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityStreamDistinct, EntityStream> Instance { get; } = new EntityStreamDistinctStepFactory();
    }
}