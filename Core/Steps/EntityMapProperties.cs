using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    /// Change the name of entity fields.
    /// </summary>
    public class EntityMapProperties : CompoundStep<IAsyncEnumerable<Entity>>, IStep<IAsyncEnumerable<Entity>>
    {
        /// <inheritdoc />
        public override async Task<Result<IAsyncEnumerable<Entity>, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var mappings = await Mappings.Run(stateMonad, cancellationToken)
                .Map(e=>e
                    .ToDictionary(x=>x.Name,x=>x.BestValue.ToString()));



            if (mappings.IsFailure) return mappings.ConvertFailure<IAsyncEnumerable<Entity>>();

            var entityStream = await EntityStream.Run(stateMonad, cancellationToken);

            if (entityStream.IsFailure) return entityStream.ConvertFailure<IAsyncEnumerable<Entity>>();


            var newEntityStream = entityStream.Value
                .Select(e=> ChangeHeader(e, mappings.Value));


            return Result.Success<IAsyncEnumerable<Entity>, IError>(newEntityStream);

            static Entity ChangeHeader(Entity entity, IReadOnlyDictionary<string, string> mappings)
            {
                var builder = entity.Dictionary.ToBuilder();
                var changed = false;

                foreach (var property in entity)
                {
                    if (mappings.TryGetValue(property.Name, out var newKey))
                    {
                        builder.Remove(property.Name);
                        var newProperty = new EntityProperty(newKey, property.BaseValue, property.NewValue, property.Order);
                        builder.Add(newKey, newProperty);
                        changed = true;
                    }
                }

                if (!changed) return entity;
                return new Entity(builder.ToImmutable());
            }

        }

        /// <summary>
        /// The stream of entities to change the field names of.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<IAsyncEnumerable<Entity>> EntityStream { get; set; } = null!;

        /// <summary>
        /// An entity containing mappings
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<Entity> Mappings { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityMapPropertiesStepFactory.Instance;
    }


    /// <summary>
    /// Change the name of entity fields.
    /// </summary>
    public class EntityMapPropertiesStepFactory : SimpleStepFactory<EntityMapProperties, IAsyncEnumerable<Entity>>
    {
        private EntityMapPropertiesStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityMapProperties, IAsyncEnumerable<Entity>> Instance { get; } = new EntityMapPropertiesStepFactory();
    }


}
