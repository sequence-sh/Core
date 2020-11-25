using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Entity = Reductech.EDR.Core.Entities.Entity;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Returns a copy of the entity with this property set.
    /// Will add a new property if the property is not already present.
    /// </summary>
    public sealed class SetProperty : CompoundStep<Entity>
    {
        /// <inheritdoc />
        public override async Task<Result<Entity, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var entity = await Entity.Run(stateMonad, cancellationToken);
            if (entity.IsFailure) return entity;

            var property = await Property.Run(stateMonad, cancellationToken);
            if (property.IsFailure) return property.ConvertFailure<Entity>();

            var value = await Value.Run(stateMonad, cancellationToken);
            if (value.IsFailure) return value.ConvertFailure<Entity>();

            var entityValue = EntityValue.Create(value.ToString());

            var newEntity = entity.Value.WithField(property.Value, entityValue);

            return newEntity;
        }

        /// <summary>
        /// The entity to set the property on.
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<Entity> Entity { get; set; } = null!;

        /// <summary>
        /// The name of the property to set.
        /// </summary>
        [StepProperty(Order = 2)]
        [Required]
        public IStep<string> Property { get; set; } = null!;

        /// <summary>
        /// The new value of the property to set.
        /// </summary>
        [StepProperty(Order = 3)]
        [Required]
        public IStep<object> Value { get; set; } = null!;


        /// <inheritdoc />
        public override IStepFactory StepFactory => SetPropertyStepFactory.Instance;
    }

    /// <summary>
    /// Returns a copy of the entity with this property set.
    /// Will add a new property if the property is not already present.
    /// </summary>
    public sealed class SetPropertyStepFactory : SimpleStepFactory<SetProperty, Entity>
    {
        private SetPropertyStepFactory() {}

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<SetProperty, Entity> Instance { get; } = new SetPropertyStepFactory();
    }
}