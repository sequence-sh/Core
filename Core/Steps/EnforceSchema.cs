using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
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
public sealed class EnforceSchema : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entityStream = await EntityStream.Run(stateMonad, cancellationToken);

        if (entityStream.IsFailure)
            return entityStream.ConvertFailure<Array<Entity>>();

        var schemaEntity = await Schema.Run(stateMonad, cancellationToken);

        if (schemaEntity.IsFailure)
            return schemaEntity.ConvertFailure<Array<Entity>>();

        var schema = Entities.Schema
            .TryCreateFromEntity(schemaEntity.Value)
            .MapError(e => e.WithLocation(this));

        if (schema.IsFailure)
            return schema.ConvertFailure<Array<Entity>>();

        Maybe<ErrorBehaviour> errorBehaviour;

        if (ErrorBehaviour == null)
            errorBehaviour = Maybe<ErrorBehaviour>.None;
        else
        {
            var errorBehaviourResult = await ErrorBehaviour.Run(stateMonad, cancellationToken);

            if (errorBehaviourResult.IsFailure)
                return errorBehaviourResult.ConvertFailure<Array<Entity>>();

            errorBehaviour = Maybe<ErrorBehaviour>.From(errorBehaviourResult.Value);
        }

        var newStream = entityStream.Value.SelectMany(ApplySchema);

        return newStream;

        async IAsyncEnumerable<Entity> ApplySchema(Entity entity)
        {
            await ValueTask.CompletedTask;
            var result = schema.Value.ApplyToEntity(entity, stateMonad.Logger, errorBehaviour);

            if (result.IsFailure)
                throw new ErrorException(result.Error.WithLocation(this));

            if (result.Value.HasValue)
                yield return result.Value.Value;
        }
    }

    /// <summary>
    /// Entities to enforce the schema on
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<Entity>> EntityStream { get; set; } = null!;

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
    [DefaultValueExplanation("Use the ErrorBehaviour defined in the schema")]
    public IStep<ErrorBehaviour>? ErrorBehaviour { get; set; } = null;

    /// <inheritdoc />
    public override IStepFactory StepFactory => EnforceSchemaStepFactory.Instance;
}

/// <summary>
/// Enforce that the schema is valid for all entities
/// </summary>
public sealed class EnforceSchemaStepFactory : SimpleStepFactory<EnforceSchema, Array<Entity>>
{
    private EnforceSchemaStepFactory() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static SimpleStepFactory<EnforceSchema, Array<Entity>> Instance { get; } =
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
