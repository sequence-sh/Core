using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Checks if an entity has a particular property.
    /// </summary>
    public sealed class EntityHasProperty : CompoundStep<bool>
    {
        /// <inheritdoc />
        public override async Task<Result<bool, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var entity = await Entity.Run(stateMonad, cancellationToken);
            if (entity.IsFailure) return entity.ConvertFailure<bool>();

            var property = await Property.Run(stateMonad, cancellationToken);
            if (property.IsFailure) return property.ConvertFailure<bool>();

            var r = entity.Value.TryGetValue(property.Value, out _);

            return r;
        }

        /// <summary>
        /// The entity to check the property on.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<Entity> Entity { get; set; } = null!;

        /// <summary>
        /// The name of the property to check.
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<string> Property { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityHasPropertyStepFactory.Instance;
    }

    /// <summary>
    /// Checks if an entity has a particular property.
    /// </summary>
    public sealed class EntityHasPropertyStepFactory : SimpleStepFactory<EntityHasProperty,bool>
    {
        private EntityHasPropertyStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityHasProperty, bool> Instance { get; } = new EntityHasPropertyStepFactory();
    }
}