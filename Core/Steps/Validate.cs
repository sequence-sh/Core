using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Logging;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Validate that the schema is valid for all entities.
/// Does not evaluate the entity stream.
/// For more information on schemas please see the
/// [documentation](https://docs.reductech.io/edr/how-to/scl/schemas.html).
/// </summary>
[Alias("SchemaValidate")]
public sealed class Validate : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(
            EntityStream,
            Schema.WrapStep(StepMaps.ConvertToSchema(Schema)),
            ErrorBehavior,
            cancellationToken
        );

        if (r.IsFailure)
            return r.ConvertFailure<Array<Entity>>();

        var (entityStream, schema, errorBehavior) = r.Value;

        var newStream = entityStream.SelectMany(ApplySchema);

        return newStream;

        async IAsyncEnumerable<Entity> ApplySchema(Entity entity)
        {
            await ValueTask.CompletedTask;
            var jsonElement = entity.ToJsonElement();

            var result = schema.Validate(
                jsonElement,
                SchemaExtensions.DefaultValidationOptions
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
                                    result.GetErrorMessages()
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
                        foreach (var errorMessage in result.GetErrorMessages())
                        {
                            LogWarning(errorMessage);
                        }

                        break;
                    }
                    case Enums.ErrorBehavior.Warning:
                    {
                        foreach (var errorMessage in result.GetErrorMessages())
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
    [DefaultValueExplanation("Fail")]
    public IStep<ErrorBehavior> ErrorBehavior { get; set; } =
        new EnumConstant<ErrorBehavior>(Enums.ErrorBehavior.Fail);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<Validate, Array<Entity>>();
}

}
