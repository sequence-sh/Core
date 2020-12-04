using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Create an entity stream from an array of entities.
    /// </summary>
    public sealed class EntityStreamCreate : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task<Result<EntityStream, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var entities = new List<Entity>();

            foreach (var element in Elements)
            {
                var r = await element.Run(stateMonad, cancellationToken);
                if (r.IsFailure) return r.ConvertFailure<EntityStream>();
                entities.Add(r.Value);
            }

            var stream = EntityStream.Create(entities);

            return stream;
        }

        /// <summary>
        /// The elements of the array.
        /// </summary>
        [StepListProperty]
        [Required]
        public IReadOnlyList<IStep<Entity>> Elements { get; set; } = null!;


        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityStreamCreateStepFactory.Instance;
    }

    /// <summary>
    /// Create an entity stream from an array of entities.
    /// </summary>
    public sealed class EntityStreamCreateStepFactory : SimpleStepFactory<EntityStreamCreate, EntityStream>
    {
        private EntityStreamCreateStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityStreamCreate, EntityStream> Instance { get; } = new EntityStreamCreateStepFactory();
    }
}