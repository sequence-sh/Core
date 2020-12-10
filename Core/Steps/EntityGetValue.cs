using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Gets the value of a property from an entity
    /// </summary>
    public sealed class EntityGetValue : CompoundStep<StringStream>
    {
        /// <inheritdoc />
        public override async Task<Result<StringStream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var entity = await Entity.Run(stateMonad, cancellationToken);

            if (entity.IsFailure) return entity.ConvertFailure<StringStream>();

            var propertyResult = await Property.Run(stateMonad, cancellationToken);

            if (propertyResult.IsFailure) return propertyResult.ConvertFailure<StringStream>();

            var propertyName = await propertyResult.Value.GetStringAsync();



            if (!entity.Value.TryGetValue(propertyName, out var ev) || ev == null)
                ev = EntityValue.Create(null, null);


            var resultString = ev.Value.Match(_ => "", v => v.ToString(), vs => string.Join(",", vs));

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