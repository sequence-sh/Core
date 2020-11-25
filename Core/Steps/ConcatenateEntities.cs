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

            var preserveOrderResult = await PreserveOrder.Run(stateMonad, cancellationToken);
            if (preserveOrderResult.IsFailure) return preserveOrderResult.ConvertFailure<EntityStream>();


            if (!preserveOrderResult.Value)
                return EntityStream.Combine(streamsResult.Value);

            var result = await EntityStream.Concatenate(streamsResult.Value, cancellationToken);
            return result;

        }

        /// <summary>
        /// The streams to concatenate
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<EntityStream>> Streams { get; set; } = null!;

        /// <summary>
        /// Whether to preserve order.
        /// </summary>
        [StepProperty]
        [DefaultValueExplanation("False")]
        public IStep<bool> PreserveOrder { get; set; } = new Constant<bool>(false);

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