using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
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
    public sealed class EntityStreamConcat : CompoundStep<IAsyncEnumerable<Entity>>
    {
        /// <inheritdoc />
        public override async Task<Result<IAsyncEnumerable<Entity>, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var streamsResult = await EntityStreams.Run(stateMonad, cancellationToken);
            if (streamsResult.IsFailure) return streamsResult.ConvertFailure<IAsyncEnumerable<Entity>>();

            var result = Concatenate(streamsResult.Value, cancellationToken);
            return Result.Success<IAsyncEnumerable<Entity>, IError>(result);
        }

        static async IAsyncEnumerable<T> Concatenate<T>(IAsyncEnumerable<IAsyncEnumerable<T>> enumerables, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var enumerable in enumerables.WithCancellation(cancellationToken))
            await foreach (var e in enumerable.WithCancellation(cancellationToken))
                yield return e;
        }


        /// <summary>
        /// The streams to concatenate
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<IAsyncEnumerable<IAsyncEnumerable<Entity>>> EntityStreams { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityStreamConcatStepFactory.Instance;
    }

    /// <summary>
    /// Concatenates streams of entities
    /// </summary>
    public sealed class EntityStreamConcatStepFactory : SimpleStepFactory<EntityStreamConcat, IAsyncEnumerable<Entity>>
    {
        private EntityStreamConcatStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityStreamConcat, IAsyncEnumerable<Entity>> Instance { get; } = new EntityStreamConcatStepFactory();
    }

}