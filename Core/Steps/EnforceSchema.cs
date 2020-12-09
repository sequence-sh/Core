using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Type = System.Type;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Enforce that the schema is valid for all entities
    /// </summary>
    public sealed class EnforceSchema : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task<Result<EntityStream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var entityStream = await EntityStream.Run(stateMonad, cancellationToken);
            if (entityStream.IsFailure) return entityStream.ConvertFailure<EntityStream>();

            var schemaEntity = await Schema.Run(stateMonad, cancellationToken);
            if (schemaEntity.IsFailure) return schemaEntity.ConvertFailure<EntityStream>();

            var schema = Entities.Schema.TryCreateFromEntity(schemaEntity.Value).MapError(e=>e.WithLocation(this));

            if (schema.IsFailure) return schema.ConvertFailure<EntityStream>();


            var errorBehaviour = await ErrorBehaviour.Run(stateMonad, cancellationToken);
            if (errorBehaviour.IsFailure) return errorBehaviour.ConvertFailure<EntityStream>();


            var newStream = entityStream.Value.ApplyMaybe(ApplySchema);

            return newStream;


            Maybe<Entity> ApplySchema(Entity entity)
            {
                var r = schema.Value.ApplyToEntity(entity);

                if (r.IsSuccess) return r.Value;

                switch (errorBehaviour.Value)
                {
                    case Steps.ErrorBehaviour.Fail:
                        {
                            throw new ErrorException(r.Error.WithLocation(this));
                        }
                    case Steps.ErrorBehaviour.Warning:
                    {
                        stateMonad.Logger.LogWarning(r.Error.AsString);
                        return Maybe<Entity>.None;
                    }
                    case Steps.ErrorBehaviour.Ignore: return Maybe<Entity>.None;
                    default:
                        throw new InvalidEnumArgumentException(nameof(errorBehaviour), (int) errorBehaviour.Value, typeof(ErrorBehaviour));
                }
            }

        }


        /// <summary>
        /// Entities to enforce the schema on
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<EntityStream> EntityStream { get; set; } = null!;

        /// <summary>
        /// The schema to enforce.
        /// This must be an entity with the properties of a schema.
        /// All other properties will be ignored.
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<Entity> Schema { get; set; } = null!;

        /// <summary>
        /// How to behave if an error occurs.
        /// </summary>
        [StepProperty(3)]
        [DefaultValueExplanation("Fail")]
        public IStep<ErrorBehaviour> ErrorBehaviour { get; set; } = new Constant<ErrorBehaviour>(Steps.ErrorBehaviour.Fail);

        /// <inheritdoc />
        public override IStepFactory StepFactory => EnforceSchemaStepFactory.Instance;
    }

    /// <summary>
    /// Enforce that the schema is valid for all entities
    /// </summary>
    public sealed class EnforceSchemaStepFactory : SimpleStepFactory<EnforceSchema, EntityStream>
    {
        private EnforceSchemaStepFactory() {}

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<EnforceSchema, EntityStream> Instance { get; } = new EnforceSchemaStepFactory();
    }

    /// <summary>
    /// How to respond to a data error
    /// </summary>
    public enum ErrorBehaviour
    {
        /// <summary>
        /// Stop the process on error
        /// </summary>
        Fail,
        /// <summary>
        /// Log a warning message on error
        /// </summary>
        Warning,
        /// <summary>
        /// Ignore errors
        /// </summary>
        Ignore
    }
}