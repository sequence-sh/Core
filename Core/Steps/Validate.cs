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
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Logging;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

///// <summary>
///// Attempts to transform entities in the stream so that they match the schema.
/////
///// Will transform strings into ints, datetimes, booleans, array, or nulls where appropriate
///// 
///// </summary>
//public sealed class Transform : CompoundStep<Array<Entity>>
//{
//    /// <inheritdoc />
//    protected override Task<Result<Array<Entity>, IError>> Run(
//        IStateMonad stateMonad,
//        CancellationToken cancellationToken)
//    {
//        throw new NotImplementedException();
//    }

//    /// <summary>
//    /// Entities to transform with the schema
//    /// </summary>
//    [StepProperty(1)]
//    [Required]
//    public IStep<Array<Entity>> EntityStream { get; set; } = null!;

//    /// <summary>
//    /// The schema to transform into
//    /// </summary>
//    [StepProperty(2)]
//    [Required]
//    public IStep<Entity> Schema { get; set; } = null!;

//    /// <summary>
//    /// ISO 8601 Date Formats to use for strings representing dates
//    /// </summary>
//    [StepProperty(3)]
//    [DefaultValueExplanation("No Date Input")]
//    public IStep<OneOf<StringStream, Array<StringStream>, Entity>>? DateInputFormats { get; set; } =
//        null;

//    /// <summary>
//    /// Strings which represent truth
//    /// </summary>
//    [StepProperty(3)]
//    [DefaultValueExplanation("True, Yes, or 1")]
//    public IStep<OneOf<StringStream, Array<StringStream>, Entity>>? BooleanTrueFormats { get; set; }
//        = new OneOfStep<StringStream, Array<StringStream>, Entity>(
//            ArrayNew<StringStream>.CreateArray(
//                new[] { "True", "Yes", "1" }.Select(
//                        x => new StringConstant(new StringStream(x)) as IStep<StringStream>
//                    )
//                    .ToList()
//            )
//        );

//    /// <inheritdoc />
//    public override IStepFactory StepFactory { get; } =
//        new SimpleStepFactory<Transform, Array<Entity>>();
//}

/// <summary>
/// Validate that the schema is valid for all entities.
/// Does not evaluate the entity stream.
/// </summary>
public sealed class Validate : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(
            EntityStream,
            Schema,
            ErrorBehavior.WrapNullable(),
            cancellationToken
        );

        if (r.IsFailure)
            return r.ConvertFailure<Array<Entity>>();

        var (entityStream, schemaEntity, errorBehavior) = r.Value;

        JsonSchema schema;

        try
        {
            schema = JsonSchema.FromText(schemaEntity.ToJsonElement().GetRawText());
        }
        catch (Exception e)
        {
            return Result.Failure<Array<Entity>, IError>(
                ErrorCode.Unknown.ToErrorBuilder(e.Message).WithLocation(this)
            );
        }

        ValidationOptions validationOptions = new()
        {
            OutputFormat = OutputFormat.Verbose, RequireFormatValidation = true,
        };

        var newStream = entityStream.SelectMany(ApplySchema);

        return newStream;

        async IAsyncEnumerable<Entity> ApplySchema(Entity entity)
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
                switch (errorBehavior.GetValueOrDefault())
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
                    default:
                        throw new ArgumentOutOfRangeException(
                            errorBehavior.GetValueOrDefault().ToString()
                        );
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
    /// Entities to validate with the schema
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<Entity>> EntityStream { get; set; } = null!;

    /// <summary>
    /// The Json Schema to validate.
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<Entity> Schema { get; set; } = null!;

    /// <summary>
    /// How to behave if an error occurs.
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("Use the ErrorBehavior defined in the schema")]
    public IStep<ErrorBehavior>? ErrorBehavior { get; set; } = null;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<Validate, Array<Entity>>();
}

}
