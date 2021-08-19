using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        var properties = GetSchemaProperties(entities);

        if (properties.IsFailure)
            return properties.MapError(x => x.WithLocation(this)).ConvertFailure<Entity>();

        var schema = new Schema
        {
            Name                    = schemaName,
            ExtraProperties         = extraPropertyBehavior,
            DefaultErrorBehavior    = errorBehavior,
            Properties              = properties.Value,
            DefaultDateInputFormats = inputFormats.Unwrap(),
            DefaultDateOutputFormat = outputFormat.Unwrap()
        };

        return schema.ConvertToEntity();
    }

    private static Result<ImmutableSortedDictionary<string, SchemaProperty>, IErrorBuilder>
        GetSchemaProperties(IReadOnlyCollection<Entity> entities)
    {
        var pairs     = entities.SelectMany(GetSchemaPropertyPairs).ToList();
        var failures1 = pairs.Where(x => x.IsFailure).ToList();

        if (failures1.Any())
            return Result.Failure<ImmutableSortedDictionary<string, SchemaProperty>, IErrorBuilder>(
                ErrorBuilderList.Combine(failures1.Select(x => x.Error))
            );

        var result = pairs
            .Select(x => x.Value)
            .GroupBy(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase)
            .Select(x => (x.Key, prop: SchemaProperty.Combine(x.Key, x, entities.Count)))
            .ToList();

        var failures2 = result.Select(x => x.prop).Where(x => x.IsFailure).ToList();

        if (failures2.Any())
            return Result.Failure<ImmutableSortedDictionary<string, SchemaProperty>, IErrorBuilder>(
                ErrorBuilderList.Combine(failures2.Select(x => x.Error))
            );

        return result
            .Where(x => x.prop.Value.HasValue)
            .ToImmutableSortedDictionary(x => x.Key, x => x.prop.Value.Value);
    }

    private static IEnumerable<Result<KeyValuePair<string, Maybe<SchemaProperty>>, IErrorBuilder>>
        GetSchemaPropertyPairs(Entity entity)
    {
        foreach (var entityProperty in entity.Dictionary.Values)
        {
            var schemaPropertyResult =
                entityProperty.BestValue.TryCreateSchemaProperty(entityProperty.Name);

            yield return new KeyValuePair<string, Maybe<SchemaProperty>>(
                entityProperty.Name,
                schemaPropertyResult.Value
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
