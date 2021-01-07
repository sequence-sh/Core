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
    [Alias("From")]
    public sealed class EntityGetValue : CompoundStep<StringStream>
    {
        /// <inheritdoc />
        protected override async Task<Result<StringStream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var entity = await Entity.Run(stateMonad, cancellationToken);

            if (entity.IsFailure) return entity.ConvertFailure<StringStream>();

            var propertyResult = await Property.Run(stateMonad, cancellationToken)
                    .Map(x=>x.GetStringAsync());

            if (propertyResult.IsFailure) return propertyResult.ConvertFailure<StringStream>();


            var entityValue = entity.Value.TryGetValue(propertyResult.Value)
                .Map(x=>x.GetString());

            string resultString = entityValue.HasValue ? entityValue.Value : "";

            var resultStream = new StringStream(resultString);

            return resultStream;
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
        [Alias("Get")]
        public IStep<StringStream> Property { get; set; } = null!;


        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityGetValueStepFactory.Instance;
    }

    /// <summary>
    /// Gets the value of a property from an entity
    /// </summary>
    public sealed class EntityGetValueStepFactory : SimpleStepFactory<EntityGetValue, StringStream>
    {
        private EntityGetValueStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityGetValue, StringStream> Instance { get; } = new EntityGetValueStepFactory();
    }
}