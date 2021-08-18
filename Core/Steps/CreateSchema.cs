using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Create a schema from an Arrayof Entity
/// </summary>
public sealed class CreateSchema : CompoundStep<Entity>
{
    /// <inheritdoc />
    protected override async Task<Result<Entity, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var stepResult = await stateMonad.RunStepsAsync(
            Array.WrapArray(),
            SchemaName.WrapStringStream(),
            ExtraPropertyBehaviour,
            DefaultErrorBehaviour,
            DefaultDateInputFormats.WrapNullable(StepMaps.Array(StepMaps.String())),
            DefaultDateOutputFormat.WrapNullable(StepMaps.String()),
            cancellationToken
        );

        if (stepResult.IsFailure)
            return stepResult.ConvertFailure<Entity>();

        var (entities, schemaName, extraPropertyBehavior, errorBehavior, inputFormats, outputFormat
            ) = stepResult.Value;

        var schema = new Schema
        {
            Name                 = schemaName,
            ExtraProperties      = extraPropertyBehavior,
            DefaultErrorBehavior = errorBehavior,
            Properties           = GetSchemaProperties(entities)
        };

        if (inputFormats.HasValue)
            schema.DefaultDateInputFormats = inputFormats.Value;

        if (outputFormat.HasValue)
            schema.DefaultDateOutputFormat = outputFormat.Value;

        return schema.ConvertToEntity();
    }

    private static Dictionary<string, SchemaProperty> GetSchemaProperties(
        IReadOnlyList<Entity> entities)
    {
        return
            entities.SelectMany(GetSchemaPropertyPairs)
                .GroupBy(x => x.Key, x => x.Value)
                .Select(x => (x.Key, prop: SchemaProperty.Combine(x, entities.Count)))
                .Where(x => x.prop.HasValue)
                .ToDictionary(x => x.Key, x => x.prop.Value);
    }

    private static IEnumerable<KeyValuePair<string, Maybe<SchemaProperty>>> GetSchemaPropertyPairs(
        Entity entity)
    {
        foreach (var entityProperty in entity.Dictionary.Values)
        {
            yield return new KeyValuePair<string, Maybe<SchemaProperty>>(
                entityProperty.Name,
                entityProperty.BestValue.CreateSchemaProperty()
            );
        }
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<CreateSchema, Entity>();

    /// <summary>
    /// The array to get the lat element of
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<Entity>> Array { get; set; } = null!;

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
    [DefaultValueExplanation(nameof(ExtraPropertyBehavior.Fail))]
    public IStep<ExtraPropertyBehavior> ExtraPropertyBehaviour { get; set; } =
        new EnumConstant<ExtraPropertyBehavior>(ExtraPropertyBehavior.Fail);

    /// <summary>
    /// The default error behavior.
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation(nameof(ErrorBehavior.Fail))]
    public IStep<ErrorBehavior> DefaultErrorBehaviour { get; set; } =
        new EnumConstant<ErrorBehavior>(ErrorBehavior.Fail);

    /// <summary>
    /// The allowed formats for dates.
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("")]
    public IStep<Array<StringStream>>? DefaultDateInputFormats { get; set; } = null!;

    /// <summary>
    /// The output format for dates.
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("")]
    public IStep<StringStream>? DefaultDateOutputFormat { get; set; } = null;
}

}
