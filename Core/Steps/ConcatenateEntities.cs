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
    public sealed class ConcatenateEntities : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task<Result<EntityStream, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var streamsResult = await Streams.Run(stateMonad, cancellationToken);
            if (streamsResult.IsFailure) return streamsResult.ConvertFailure<EntityStream>();

            var result = EntityStream.Concatenate(streamsResult.Value);
            return result;

        }

        /// <summary>
        /// The streams to concatenate
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<EntityStream>> Streams { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => ConcatenateEntitiesStepFactory.Instance;
    }

    /// <summary>
    /// Concatenates streams of entities
    /// </summary>
    public sealed class ConcatenateEntitiesStepFactory : SimpleStepFactory<ConcatenateEntities, EntityStream>
    {
        private ConcatenateEntitiesStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ConcatenateEntities, EntityStream> Instance { get; } = new ConcatenateEntitiesStepFactory();
    }

}