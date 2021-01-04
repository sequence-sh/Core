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
    /// Concatenates streams of entities
    /// </summary>
    public sealed class EntityStreamConcat : CompoundStep<Core.Sequence<Entity>>
    {
        /// <inheritdoc />
        public override async Task<Result<Core.Sequence<Entity>, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var streamsResult = await EntityStreams.Run(stateMonad, cancellationToken);
            if (streamsResult.IsFailure) return streamsResult.ConvertFailure<Core.Sequence<Entity>>();

            var result =
                streamsResult.Value.SelectMany(al =>
            {
                var asyncEnumerable = al.
                Option.IsT0 ? al.Option.AsT0.ToAsyncEnumerable() : al.Option.AsT1;
                return asyncEnumerable;
            });

            return result;
        }

        /// <summary>
        /// The streams to concatenate
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<Core.Sequence<Core.Sequence<Entity>>> EntityStreams { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityStreamConcatStepFactory.Instance;
    }

    /// <summary>
    /// Concatenates streams of entities
    /// </summary>
    public sealed class EntityStreamConcatStepFactory : SimpleStepFactory<EntityStreamConcat, Core.Sequence<Entity>>
    {
        private EntityStreamConcatStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityStreamConcat, Core.Sequence<Entity>> Instance { get; } = new EntityStreamConcatStepFactory();
    }

}