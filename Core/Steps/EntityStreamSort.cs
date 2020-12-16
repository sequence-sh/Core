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
using Reductech.EDR.Core.Parser;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Reorder entities according to their property values.
    /// Consumes the stream.
    /// </summary>
    public sealed class EntityStreamSort : CompoundStep<IAsyncEnumerable<Entity>>
    {
        /// <inheritdoc />
        public override async Task<Result<IAsyncEnumerable<Entity>, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var sortDescending = await Descending.Run(stateMonad, cancellationToken);
            if (sortDescending.IsFailure) return sortDescending.ConvertFailure<IAsyncEnumerable<Entity>>();

            var entityStreamResult = await EntityStream.Run(stateMonad, cancellationToken);
            if (entityStreamResult.IsFailure) return entityStreamResult.ConvertFailure<IAsyncEnumerable<Entity>>();

            var currentState = stateMonad.GetState().ToImmutableDictionary();

            async ValueTask<string> GetKey(Entity entity)
            {
                var scopedMonad = new ScopedStateMonad(stateMonad, currentState,
                    new KeyValuePair<VariableName, object>(VariableName.Entity, entity));

                var result = await KeySelector.Run(scopedMonad, cancellationToken)
                    .Map(x=>x.GetStringAsync());

                if (result.IsFailure)
                    throw new ErrorException(result.Error);

                return result.Value;
            }

            IOrderedAsyncEnumerable<Entity> resultStream;

            if(sortDescending.Value)
                resultStream = entityStreamResult.Value.OrderByDescendingAwait(GetKey);
            else resultStream = entityStreamResult.Value.OrderByAwait(GetKey);

            return Result.Success<IAsyncEnumerable<Entity>, IError>(resultStream);
        }

        /// <summary>
        /// The entities to sort
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<IAsyncEnumerable<Entity>> EntityStream { get; set; } = null!;

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
    public sealed class EntityStreamSortStepFactory : SimpleStepFactory<EntityStreamSort, IAsyncEnumerable<Entity>>
    {
        private EntityStreamSortStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityStreamSort, IAsyncEnumerable<Entity>> Instance { get; } = new EntityStreamSortStepFactory();
    }

}
