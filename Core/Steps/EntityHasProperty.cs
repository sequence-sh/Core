using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Parser;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Checks if an entity has a particular property.
    /// </summary>
    [Alias("DoesEntityHave")]
    public sealed class EntityHasProperty : CompoundStep<bool>
    {
        /// <inheritdoc />
        protected override async Task<Result<bool, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var entity = await Entity.Run(stateMonad, cancellationToken);
            if (entity.IsFailure) return entity.ConvertFailure<bool>();

            var propertyResult = await Property.Run(stateMonad, cancellationToken);
            if (propertyResult.IsFailure) return propertyResult.ConvertFailure<bool>();

            var propertyName = await propertyResult.Value.GetStringAsync();

            var r = entity.Value.TryGetValue(propertyName).HasValue;

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
        public IStep<StringStream> Property { get; set; } = null!;

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