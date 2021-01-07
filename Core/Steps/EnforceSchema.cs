using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Enforce that the schema is valid for all entities
/// </summary>
public sealed class EnforceSchema : CompoundStep<Core.Array<Entity>>
{
    /// <inheritdoc />
    protected override async Task<Result<Core.Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entityStream = await EntityStream.Run(stateMonad, cancellationToken);

        if (entityStream.IsFailure)
            return entityStream.ConvertFailure<Core.Array<Entity>>();

        var schemaEntity = await Schema.Run(stateMonad, cancellationToken);

        if (schemaEntity.IsFailure)
            return schemaEntity.ConvertFailure<Core.Array<Entity>>();

        var schema = Entities.Schema
            .TryCreateFromEntity(schemaEntity.Value)
            .MapError(e => e.WithLocation(this));

        if (schema.IsFailure)
            return schema.ConvertFailure<Core.Array<Entity>>();

        var errorBehaviour = await ErrorBehaviour.Run(stateMonad, cancellationToken);

        if (errorBehaviour.IsFailure)
            return errorBehaviour.ConvertFailure<Core.Array<Entity>>();

        var newStream = entityStream.Value.SelectMany(ApplySchema);

        return newStream;

        async IAsyncEnumerable<Entity> ApplySchema(Entity entity)
        {
            await ValueTask.CompletedTask;
            var result = schema.Value.ApplyToEntity(entity);

            if (result.IsSuccess)
            {
                yield return result.Value;

                yield break;
            }

            switch (errorBehaviour.Value)
            {
                case Enums.ErrorBehaviour.Fail:
                {
                    throw new ErrorException(result.Error.WithLocation(this));
                }
                case Enums.ErrorBehaviour.Warning:
                {
                    stateMonad.Logger.LogWarning(result.Error.AsString);
                    break;
                }
                case Enums.ErrorBehaviour.Ignore:
                {
                    break;
                }
                default:
                    throw new InvalidEnumArgumentException(
                        nameof(errorBehaviour),
                        (int)errorBehaviour.Value,
                        typeof(ErrorBehaviour)
                    );
            }
        }
    }

    /// <summary>
    /// Entities to enforce the schema on
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Core.Array<Entity>> EntityStream { get; set; } = null!;

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
    public IStep<ErrorBehaviour> ErrorBehaviour { get; set; } =
        new EnumConstant<ErrorBehaviour>(Enums.ErrorBehaviour.Fail);

    /// <inheritdoc />
    public override IStepFactory StepFactory => EnforceSchemaStepFactory.Instance;
}

/// <summary>
/// Enforce that the schema is valid for all entities
/// </summary>
public sealed class EnforceSchemaStepFactory : SimpleStepFactory<EnforceSchema, Core.Array<Entity>>
{
    private EnforceSchemaStepFactory() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static SimpleStepFactory<EnforceSchema, Core.Array<Entity>> Instance { get; } =
        new EnforceSchemaStepFactory();

    /// <inheritdoc />
    public override IEnumerable<Type> ExtraEnumTypes
    {
        get
        {
            yield return typeof(Multiplicity);
            yield return typeof(SchemaPropertyType);
        }
    }
}

}
