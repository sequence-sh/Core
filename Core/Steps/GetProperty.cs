﻿using System.ComponentModel.DataAnnotations;
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
    /// Get a property from an entity
    /// </summary>
    public sealed class GetProperty : CompoundStep<string>
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
                ev = EntityValue.Create(null as string);


            var resultString = ev.Value.Match(_ => "", v => v.ToString(), vs => string.Join(",", vs));

            return resultString;
        }

        /// <summary>
        /// The entity to get the property from.
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<Entity> Entity { get; set; } = null!;

        /// <summary>
        /// The name of the property to get.
        /// Returns an empty string if the property is not present.
        /// </summary>
        [StepProperty(Order = 2)]
        [Required]
        public IStep<string> Property { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => GetPropertyStepFactory.Instance;
    }

    /// <summary>
    /// Get a property from an entity
    /// </summary>
    public sealed class GetPropertyStepFactory : SimpleStepFactory<GetProperty, string>
    {
        private GetPropertyStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<GetProperty, string> Instance { get; } = new GetPropertyStepFactory();
    }
}