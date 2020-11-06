using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
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
    public class MapHeaders : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task<Result<EntityStream, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var oldHeader = await OldHeader.Run(stateMonad, cancellationToken);

            if (oldHeader.IsFailure) return oldHeader.ConvertFailure<EntityStream>();

            var newHeader = await NewHeader.Run(stateMonad, cancellationToken);

            if (newHeader.IsFailure) return newHeader.ConvertFailure<EntityStream>();

            var entityStream = await EntityStream.Run(stateMonad, cancellationToken);

            if (entityStream.IsFailure) return entityStream.ConvertFailure<EntityStream>();


            var newEntityStream = entityStream.Value.Apply(e=> ChangeHeader(e, oldHeader.Value, newHeader.Value));


            return newEntityStream;


            static Entity ChangeHeader(Entity entity, string oldHeader, string newHeader)
            {
                var pairs = new List<KeyValuePair<string, string>>();

                foreach (var grouping in entity)
                {
                    string newKey;
                    if (grouping.Key == oldHeader)
                        newKey = newHeader;
                    else newKey = grouping.Key;

                    foreach (var value in grouping)
                        pairs.Add(new KeyValuePair<string, string>(newKey, value));
                }

                return new Entity(pairs);
            }

        }

        [StepProperty]
        [Required]
        public IStep<EntityStream> EntityStream { get; set; }

        [StepProperty]
        public IStep<string> OldHeader { get; set; }

        [StepProperty]
        public IStep<string> NewHeader { get; set; }

        /// <inheritdoc />
        public override IStepFactory StepFactory => MapHeadersFactory.Instance;
    }

    public class MapHeadersFactory : SimpleStepFactory<MapHeaders, EntityStream>
    {
        private MapHeadersFactory() {}

        public static SimpleStepFactory<MapHeaders, EntityStream> Instance { get; } = new MapHeadersFactory();
    }


}
