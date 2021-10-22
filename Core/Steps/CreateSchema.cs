﻿using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Json.More;
using Json.Schema;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Create a schema from an Array of Entity
/// </summary>
public sealed class CreateSchema : CompoundStep<Entity>
{
    /// <inheritdoc />
    protected override async Task<Result<Entity, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var stepResult = await stateMonad.RunStepsAsync(
            Entities.WrapArray(),
            SchemaName.WrapStringStream(),
            AllowExtraProperties,
            cancellationToken
        );

        if (stepResult.IsFailure)
            return stepResult.ConvertFailure<Entity>();

        var (entities,
            schemaName,
            allowExtraProperties
            ) = stepResult.Value;

        var schema =
            entities.Select(x => new EntityValue.NestedEntity(x).ToSchemaNode())
                .Aggregate((a, b) => a.Combine(b))
                .ToJsonSchema();

        var jsonSchemaBuilder = new JsonSchemaBuilder()
            .Title(schemaName)
            .Type(SchemaValueType.Object)
            .AdditionalProperties(allowExtraProperties ? JsonSchema.True : JsonSchema.False);

        var props = schema.Keywords?.OfType<PropertiesKeyword>().FirstOrDefault();

        if (props is not null)
        {
            jsonSchemaBuilder.Properties(props.Properties);
        }

        var schemaEntity = Entity.Create(jsonSchemaBuilder.Build().ToJsonDocument().RootElement);
        return schemaEntity;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<CreateSchema, Entity>();

    /// <summary>
    /// The array to get the lat element of
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<Entity>> Entities { get; set; } = null!;

    /// <summary>
    /// The name of the schema to create
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("Schema")]
    public IStep<StringStream> SchemaName { get; set; } = new StringConstant("Schema");

    /// <summary>
    /// Whether properties other than the explicitly defined properties are allowed.
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("false")]
    public IStep<bool> AllowExtraProperties { get; set; } = new BoolConstant(false);
}

}
