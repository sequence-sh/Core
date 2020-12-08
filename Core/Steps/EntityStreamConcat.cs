using System.Collections.Generic;
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
    /// Concatenates streams of entities
    /// </summary>
    public sealed class EntityStreamConcat : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task<Result<EntityStream, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var streamsResult = await EntityStreams.Run(stateMonad, cancellationToken);
            if (streamsResult.IsFailure) return streamsResult.ConvertFailure<EntityStream>();

            var result = EntityStream.Concatenate(streamsResult.Value);
            return result;
        }

        /// <summary>
        /// The streams to concatenate
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<List<EntityStream>> EntityStreams { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityStreamConcatStepFactory.Instance;
    }

    /// <summary>
    /// Concatenates streams of entities
    /// </summary>
    public sealed class EntityStreamConcatStepFactory : SimpleStepFactory<EntityStreamConcat, EntityStream>
    {
        private EntityStreamConcatStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityStreamConcat, EntityStream> Instance { get; } = new EntityStreamConcatStepFactory();
    }

}