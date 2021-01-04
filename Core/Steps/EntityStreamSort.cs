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
    /// Reorder entities according to their property values.
    /// Consumes the stream.
    /// </summary>
    [Alias("SortEntityStream")]
    public sealed class EntityStreamSort : CompoundStep<Core.Sequence<Entity>>
    {
        /// <inheritdoc />
        public override async Task<Result<Core.Sequence<Entity>, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var sortDescending = await Descending.Run(stateMonad, cancellationToken);
            if (sortDescending.IsFailure) return sortDescending.ConvertFailure<Core.Sequence<Entity>>();

            var entityStreamResult = await EntityStream.Run(stateMonad, cancellationToken);
            if (entityStreamResult.IsFailure) return entityStreamResult.ConvertFailure<Core.Sequence<Entity>>();

            var currentState = stateMonad.GetState().ToImmutableDictionary();

            async ValueTask<string> GetKey(Entity entity, CancellationToken cancellation)
            {
                var scopedMonad = new ScopedStateMonad(stateMonad, currentState,
                    new KeyValuePair<VariableName, object>(VariableName.Entity, entity));

                var result = await KeySelector.Run(scopedMonad, cancellation)
                    .Map(x=>x.GetStringAsync());

                if (result.IsFailure)
                    throw new ErrorException(result.Error);

                return result.Value;
            }

            var r = entityStreamResult.Value.Sort(sortDescending.Value, GetKey);

            return r;
        }

        /// <summary>
        /// The entities to sort
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<Core.Sequence<Entity>> EntityStream { get; set; } = null!;

        /// <summary>
        /// A function that gets the key to sort by from the variable &lt;Entity&gt;
        /// To sort by multiple properties, concatenate several keys
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<StringStream> KeySelector { get; set; } = null!;

        /// <summary>
        /// Whether to sort in descending order.
        /// </summary>
        [StepProperty(3)]
        [DefaultValueExplanation("False")]
        public IStep<bool> Descending { get; set; } = new BoolConstant(false);

        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityStreamSortStepFactory.Instance;
    }

    /// <summary>
    /// Reorder entities according to their property values.
    /// Consumes the stream.
    /// </summary>
    public sealed class EntityStreamSortStepFactory : SimpleStepFactory<EntityStreamSort, Core.Sequence<Entity>>
    {
        private EntityStreamSortStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityStreamSort, Core.Sequence<Entity>> Instance { get; } = new EntityStreamSortStepFactory();
    }

}
