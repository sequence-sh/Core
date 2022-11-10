using Sequence.Core.Enums;
using Sequence.Core.Internal.Logging;

namespace Sequence.Core.Steps;

/// <summary>
/// Validate that the schema is valid for all entities.
/// Does not evaluate the entity stream.
/// For more information on schemas please see the
/// [documentation](https://sequence.sh/docs/schemas/).
/// </summary>
[Alias("SchemaValidate")]
[AllowConstantFolding]
public sealed class Validate : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Array<Entity>, IError>> Run(
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

        var rowNumber = 0;
        var newStream = entityStream.SelectMany(ApplySchema);

        return newStream;

        async IAsyncEnumerable<Entity> ApplySchema(Entity entity)
        {
            var transformRoot = new TransformRoot(rowNumber, entity);
            rowNumber += 1;
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
                if (OnInvalid is not null)
                {
                    var stateDict = stateMonad.GetState()
                        .ToImmutableDictionary();

                    foreach (var (m, l, tr) in result.GetErrorMessages(transformRoot))
                    {
                        var scoped =
                            new ScopedStateMonad(
                                stateMonad,
                                stateDict,
                                OnInvalid.VariableNameOrItem,
                                new KeyValuePair<VariableName, ISCLObject>(
                                    OnInvalid.VariableNameOrItem,
                                    Entity.Create(
                                        new (EntityNestedKey key, ISCLObject value)[]
                                        {
                                            (EntityNestedKey.Create("RowNumber"),
                                             new SCLInt(tr.RowNumber)),
                                            (EntityNestedKey.Create("ErrorMessage"),
                                             new StringStream(m)),
                                            (EntityNestedKey.Create("Location"),
                                             new StringStream(l)),
                                            (EntityNestedKey.Create("Entity"),
                                             tr.Entity),
                                        }
                                    )
                                )
                            );

                        await OnInvalid.StepTyped.Run(scoped, cancellationToken);
                    }
                }

                switch (errorBehavior.Value)
                {
                    case Enums.ErrorBehavior.Fail:
                    {
                        var errors =
                            ErrorBuilderList.Combine(
                                    result.GetErrorMessages(transformRoot)
                                        .Select(
                                            x => ErrorCode.SchemaViolated.ToErrorBuilder(
                                                x.message,
                                                x.location,
                                                transformRoot.RowNumber,
                                                transformRoot.Entity
                                            )
                                        )
                                )
                                .WithLocation(this);

                        throw new ErrorException(errors);
                    }
                    case Enums.ErrorBehavior.Error:
                    {
                        foreach (var errorMessage in result.GetErrorMessages(transformRoot))
                        {
                            LogWarning(errorMessage);
                        }

                        break;
                    }
                    case Enums.ErrorBehavior.Warning:
                    {
                        foreach (var errorMessage in result.GetErrorMessages(transformRoot))
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

        void LogWarning((string message, string location, TransformRoot transformRoot) pair)
        {
            LogSituation.SchemaViolated.Log(
                stateMonad,
                this,
                pair.message,
                pair.location,
                pair.transformRoot.RowNumber,
                pair.transformRoot.Entity
            );
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
    /// A function that does something with every entity that fails validation.  
    /// The actions defined in ErrorBehaviour will also happen.
    /// This function takes an entity with three properties: 'RowNumber', 'ErrorMessage', 'Location', and 'Entity' where 'Entity' contains the entity that failed to validate
    /// </summary>
    [FunctionProperty]
    [DefaultValueExplanation("Either this or 'Replace' must be set.")]
    public LambdaFunction<Entity, Unit>? OnInvalid { get; set; } = null!;

    /// <summary>
    /// How to behave if an error occurs.
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("Fail")]
    public IStep<SCLEnum<ErrorBehavior>> ErrorBehavior { get; set; } =
        new SCLConstant<SCLEnum<ErrorBehavior>>(
            new SCLEnum<ErrorBehavior>(Enums.ErrorBehavior.Fail)
        );

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<Validate, Array<Entity>>();
}
