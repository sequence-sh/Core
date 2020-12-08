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
    /// Gets the value of a property from an entity
    /// </summary>
    public sealed class EntityGetValue : CompoundStep<string>
    {
        /// <inheritdoc />
        public override async Task<Result<string, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var entity = await Entity.Run(stateMonad, cancellationToken);

            if (entity.IsFailure) return entity.ConvertFailure<string>();

            var property = await Property.Run(stateMonad, cancellationToken);

            if (property.IsFailure) return property.ConvertFailure<string>();

            if (!entity.Value.TryGetValue(property.Value, out var ev) || ev == null)
                ev = EntityValue.Create(null, null);


            var resultString = ev.Value.Match(_ => "", v => v.ToString(), vs => string.Join(",", vs));

            return resultString;
        }

        /// <summary>
        /// The entity to get the property from.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<Entity> Entity { get; set; } = null!;

        /// <summary>
        /// The name of the property to get.
        /// Returns an empty string if the property is not present.
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<string> Property { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityGetValueStepFactory.Instance;
    }

    /// <summary>
    /// Gets the value of a property from an entity
    /// </summary>
    public sealed class EntityGetValueStepFactory : SimpleStepFactory<EntityGetValue, string>
    {
        private EntityGetValueStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityGetValue, string> Instance { get; } = new EntityGetValueStepFactory();
    }
}