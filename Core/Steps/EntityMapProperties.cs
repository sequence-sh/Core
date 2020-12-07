using System.Collections.Generic;
using System.Collections.Immutable;
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
    public class EntityMapProperties : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task<Result<EntityStream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var mappings = await Mappings.Run(stateMonad, cancellationToken)
                .Map(e=>e
                    .Where(x=>x.Value.Value.IsT1)
                    .ToDictionary(x=>x.Key,x=>x.Value.Value.AsT1.ToString()));



            if (mappings.IsFailure) return mappings.ConvertFailure<EntityStream>();

            var entityStream = await EntityStream.Run(stateMonad, cancellationToken);

            if (entityStream.IsFailure) return entityStream.ConvertFailure<EntityStream>();


            var newEntityStream = entityStream.Value.Apply(e=> ChangeHeader(e, mappings.Value));


            return newEntityStream;


            static Entity ChangeHeader(Entity entity, IReadOnlyDictionary<string, string> mappings)
            {
                var pairs = ImmutableList<KeyValuePair<string, EntityValue>>.Empty.ToBuilder();

                foreach (var kvp in entity)
                {
                    if (mappings.TryGetValue(kvp.Key, out var newKey))
                        pairs.Add(new KeyValuePair<string, EntityValue>(newKey, kvp.Value));
                    else pairs.Add(kvp);
                }

                return new Entity(pairs.ToImmutable());
            }

        }

        /// <summary>
        /// The stream of entities to change the field names of.
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<EntityStream> EntityStream { get; set; } = null!;

        /// <summary>
        /// An entity containing mappings
        /// </summary>
        [StepProperty(Order = 2)]
        [Required]
        public IStep<Entity> Mappings { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityMapPropertiesStepFactory.Instance;
    }


    /// <summary>
    /// Change the name of entity fields.
    /// </summary>
    public class EntityMapPropertiesStepFactory : SimpleStepFactory<EntityMapProperties, EntityStream>
    {
        private EntityMapPropertiesStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityMapProperties, EntityStream> Instance { get; } = new EntityMapPropertiesStepFactory();
    }


}
