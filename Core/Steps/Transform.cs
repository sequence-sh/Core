using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Json.Schema;
using OneOf;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Logging;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Attempts to transform entities in the stream so that they match the schema.
///
/// Will transform strings into ints, datetimes, booleans, array, or nulls where appropriate
/// 
/// </summary>
public sealed class Transform : CompoundStep<Array<Entity>>
{
    //public static Result<Entity, IErrorBuilder> TryTransform(Entity entity, JsonSchema schema)
    //{
    //    //TODO improve performance

    //    HashSet<string> requiredProperties = new();

    //    if (schema.Keywords is not null)
    //        foreach (var required in schema.Keywords.OfType<RequiredKeyword>())
    //            requiredProperties.UnionWith(required.Properties);

    //    if (schema.Keywords is not null)
    //    {
    //        foreach (var properties in schema.Keywords.OfType<PropertiesKeyword>())
    //        {
    //            foreach (var (propertyName, nestedSchema) in properties.Properties)
    //            {
    //                //nestedSchema.
    //            }
    //        }
    //    }
    //}

    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var inputFormatMap = StepMaps.OneOf(
            StepMaps.String(),
            StepMaps.Array(StepMaps.String()),
            StepMaps.DoNothing<Entity>()
        );

        var stepsResult = await stateMonad.RunStepsAsync(
            EntityStream,
            Schema.WrapStep(StepMaps.ConvertToSchema(Schema)),
            ErrorBehavior,
            DateInputFormats.WrapNullable(inputFormatMap),
            BooleanTrueFormats.WrapNullable(inputFormatMap),
            BooleanFalseFormats.WrapNullable(inputFormatMap),
            NullFormats.WrapNullable(inputFormatMap),
            CaseSensitive,
            RemoveExtraProperties.WrapNullable(),
            cancellationToken
        );

        if (stepsResult.IsFailure)
            return stepsResult.ConvertFailure<Array<Entity>>();

        var (entityStream,
            schema,
            errorBehavior,
            dateInputFormats,
            boolTrueFormats,
            boolFalseFormats,
            nullFormats,
            caseSensitive,
            removeExtraMaybe) = stepsResult.Value;

        var newEntityStream = entityStream.SelectMany(TryTransform);

        return newEntityStream;

        async IAsyncEnumerable<Entity> TryTransform(Entity entity)
        {
            await ValueTask.CompletedTask;
            var jsonElement = entity.ToJsonElement();

            var result = schema.Validate(
                jsonElement,
                validationOptions
            );

            if (result.IsValid)
                yield return entity;
            else
            {
                switch (errorBehavior)
                {
                    case Enums.ErrorBehavior.Fail:
                    {
                        var errors =
                            ErrorBuilderList.Combine(
                                    GetErrorMessages(result)
                                        .Select(
                                            x => ErrorCode.SchemaViolation.ToErrorBuilder(
                                                x.message,
                                                x.location
                                            )
                                        )
                                )
                                .WithLocation(this);

                        throw new ErrorException(errors);
                    }
                    case Enums.ErrorBehavior.Error:
                    {
                        foreach (var errorMessage in GetErrorMessages(result))
                        {
                            LogWarning(errorMessage);
                        }

                        break;
                    }
                    case Enums.ErrorBehavior.Warning:
                    {
                        foreach (var errorMessage in GetErrorMessages(result))
                        {
                            LogWarning(errorMessage);
                        }

                        yield return entity;

                        break;
                    }
                    case Enums.ErrorBehavior.Skip: break;
                    case Enums.ErrorBehavior.Ignore:
                    {
                        yield return entity;

                        break;
                    }
                    default: throw new ArgumentOutOfRangeException(errorBehavior.ToString());
                }
            }
        }

        static IEnumerable<(string message, string location)> GetErrorMessages(
            ValidationResults validationResults)
        {
            if (!validationResults.IsValid)
            {
                if (validationResults.Message is not null)
                    yield return (validationResults.Message,
                                  validationResults.SchemaLocation.ToString());

                foreach (var nestedResult in validationResults.NestedResults)
                foreach (var errorMessage in GetErrorMessages(nestedResult))
                    yield return errorMessage;
            }
        }

        void LogWarning((string message, string location) pair)
        {
            LogSituation.SchemaViolation.Log(stateMonad, this, pair.message, pair.location);
        }
    }

    /// <summary>
    /// Entities to transform with the schema
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<Entity>> EntityStream { get; set; } = null!;

    /// <summary>
    /// The schema to transform into
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<Entity> Schema { get; set; } = null!;

    /// <summary>
    /// How to behave if an error occurs.
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("Fail")]
    public IStep<ErrorBehavior> ErrorBehavior { get; set; } =
        new EnumConstant<ErrorBehavior>(Enums.ErrorBehavior.Fail);

    /// <summary>
    /// ISO 8601 Date Formats to use for strings representing dates
    /// This can either be a string, and array of string, or an entity mapping field names to strings or arrays of string
    /// </summary>
    [StepProperty()]
    [DefaultValueExplanation("No Date Input")]
    public IStep<OneOf<StringStream, Array<StringStream>, Entity>>? DateInputFormats { get; set; } =
        null;

    /// <summary>
    /// Strings which represent truth.
    /// This can either be a string, and array of string, or an entity mapping field names to strings or arrays of string
    /// </summary>
    [StepProperty()]
    [DefaultValueExplanation("True, Yes, or 1")]
    public IStep<OneOf<StringStream, Array<StringStream>, Entity>>? BooleanTrueFormats { get; set; }
        = new OneOfStep<StringStream, Array<StringStream>, Entity>(
            ArrayNew<StringStream>.CreateArray(
                new[] { "True", "Yes", "1" }.Select(
                        x => new StringConstant(new StringStream(x)) as IStep<StringStream>
                    )
                    .ToList()
            )
        );

    /// <summary>
    /// Strings which represent falsity
    /// This can either be a string, and array of string, or an entity mapping field names to strings or arrays of string
    /// </summary>
    [StepProperty()]
    [DefaultValueExplanation("False, No, or 0")]
    public IStep<OneOf<StringStream, Array<StringStream>, Entity>>? BooleanFalseFormats
    {
        get;
        set;
    }
        = new OneOfStep<StringStream, Array<StringStream>, Entity>(
            ArrayNew<StringStream>.CreateArray(
                new[] { "False", "No", "0" }.Select(
                        x => new StringConstant(new StringStream(x)) as IStep<StringStream>
                    )
                    .ToList()
            )
        );

    /// <summary>
    /// Strings which represent null
    /// This can either be a string, and array of string, or an entity mapping field names to strings or arrays of string
    /// </summary>
    [StepProperty()]
    [DefaultValueExplanation("Null")]
    public IStep<OneOf<StringStream, Array<StringStream>, Entity>>? NullFormats { get; set; }
        = new OneOfStep<StringStream, Array<StringStream>, Entity>(
            ArrayNew<StringStream>.CreateArray(
                new[] { "Null" }.Select(
                        x => new StringConstant(new StringStream(x)) as IStep<StringStream>
                    )
                    .ToList()
            )
        );

    /// <summary>
    /// Strings which can be used to delimit arrays
    /// This can either be a string, and array of string, or an entity mapping field names to strings or arrays of string
    /// </summary>
    [StepProperty()]
    [DefaultValueExplanation("No Delimiter")]
    public IStep<OneOf<StringStream, Array<StringStream>, Entity>>? ArrayDelimiters { get; set; }
        = new OneOfStep<StringStream, Array<StringStream>, Entity>(
            ArrayNew<StringStream>.CreateArray(
                new[] { "Null" }.Select(
                        x => new StringConstant(new StringStream(x)) as IStep<StringStream>
                    )
                    .ToList()
            )
        );

    /// <summary>
    /// Whether transformations are case sensitive
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("False")]
    public IStep<bool> CaseSensitive { get; set; } = new BoolConstant(false);

    /// <summary>
    /// Whether additional properties outside the schema should be removed
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("True if the Schema does not allow extra properties")]
    public IStep<bool>? RemoveExtraProperties { get; set; } = null;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<Transform, Array<Entity>>();
}

}
