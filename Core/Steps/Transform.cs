using Reductech.Sequence.Core.Enums;
using Reductech.Sequence.Core.Internal.Logging;

namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Attempts to transform entities in the stream so that they match the schema.
///
/// Will transform strings into ints, datetimes, booleans, array, or nulls where appropriate
///
/// For more information on schemas please see the
/// [documentation](https://docs.reductech.io/sequence/how-to/scl/schemas.html). 
/// </summary>
[Alias("SchemaTransform")]
public sealed class Transform : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var inputFormatMap =
            StepMaps.OneOf(
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
            ArrayDelimiters.WrapNullable(inputFormatMap),
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
            delimiters,
            caseSensitive,
            removeExtraMaybe) = stepsResult.Value;

        var topNode = SchemaNode.Create(schema);

        TransformSettings transformSettings = new(
            Formatter.Create(dateInputFormats),
            Formatter.Create(boolTrueFormats),
            Formatter.Create(boolFalseFormats),
            Formatter.Create(nullFormats),
            Formatter.Create(delimiters),
            caseSensitive.Value,
            removeExtraMaybe.Map(x => x.Value)
        );

        var newEntityStream = entityStream.SelectMany(TryTransform);

        return newEntityStream;

        async IAsyncEnumerable<Entity> TryTransform(Entity entity)
        {
            await ValueTask.CompletedTask;

            var result = topNode.TryTransform(
                "",
                new Entity(entity),
                transformSettings
            );

            if (result.IsSuccess)
            {
                if (result.Value.HasValue)
                {
                    if (result.Value.GetValueOrThrow() is Entity ne)
                        yield return ne;
                    else
                        yield return new Entity(
                            new[]
                            {
                                new EntityProperty(
                                    Entity.PrimitiveKey,
                                    result.Value.GetValueOrThrow(),
                                    1
                                )
                            }.ToImmutableDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase)
                        );
                }
                else
                    yield return entity; //no change
            }

            else
            {
                switch (errorBehavior.Value)
                {
                    case Enums.ErrorBehavior.Fail:
                    {
                        throw new ErrorException(result.Error.WithLocation(this));
                    }
                    case Enums.ErrorBehavior.Error:
                    {
                        LogWarning(result.Error);

                        break;
                    }
                    case Enums.ErrorBehavior.Warning:
                    {
                        LogWarning(result.Error);
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

        void LogWarning(IErrorBuilder errorBuilder)
        {
            if (errorBuilder is ErrorBuilderList ebl)
            {
                foreach (var errorBuilder1 in ebl.ErrorBuilders)
                {
                    LogWarning(errorBuilder1);
                }
            }
            else if (errorBuilder is ErrorBuilder(var errorCodeBase, var errorData)
                  && errorCodeBase == ErrorCode.SchemaViolation
                  && errorData is ErrorData.ObjectData objData)
            {
                LogSituation.SchemaViolation.Log(stateMonad, this, objData.Arguments);
            }
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
    public IStep<SCLEnum<ErrorBehavior>> ErrorBehavior { get; set; } =
        new SCLConstant<SCLEnum<ErrorBehavior>>(
            new SCLEnum<ErrorBehavior>(Enums.ErrorBehavior.Fail)
        );

    /// <summary>
    /// ISO 8601 Date Formats to use for strings representing dates
    /// This can either be a string, and array of string, or an entity mapping field names to strings or arrays of string
    /// </summary>
    [StepProperty()]
    [DefaultValueExplanation("No Date Input")]
    public IStep<SCLOneOf<StringStream, Array<StringStream>, Entity>>? DateInputFormats
    {
        get;
        set;
    } =
        null;

    /// <summary>
    /// Strings which represent truth.
    /// This can either be a string, and array of string, or an entity mapping field names to strings or arrays of string
    /// </summary>
    [StepProperty()]
    [DefaultValueExplanation("True, Yes, or 1")]
    public IStep<SCLOneOf<StringStream, Array<StringStream>, Entity>>? BooleanTrueFormats
    {
        get;
        set;
    }
        = new OneOfStep<StringStream, Array<StringStream>, Entity>(
            ArrayNew<StringStream>.CreateArray(
                new[] { "True", "Yes", "1" }.Select(
                        x => new SCLConstant<StringStream>(new StringStream(x)) as
                            IStep<StringStream>
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
    public IStep<SCLOneOf<StringStream, Array<StringStream>, Entity>>? BooleanFalseFormats
    {
        get;
        set;
    }
        = new OneOfStep<StringStream, Array<StringStream>, Entity>(
            ArrayNew<StringStream>.CreateArray(
                new[] { "False", "No", "0" }.Select(
                        x => new SCLConstant<StringStream>(new StringStream(x)) as
                            IStep<StringStream>
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
    public IStep<SCLOneOf<StringStream, Array<StringStream>, Entity>>? NullFormats { get; set; }
        = new OneOfStep<StringStream, Array<StringStream>, Entity>(
            ArrayNew<StringStream>.CreateArray(
                new[] { "Null" }.Select(
                        x => new SCLConstant<StringStream>(new StringStream(x)) as
                            IStep<StringStream>
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
    public IStep<SCLOneOf<StringStream, Array<StringStream>, Entity>>? ArrayDelimiters { get; set; }
        = new OneOfStep<StringStream, Array<StringStream>, Entity>(
            ArrayNew<StringStream>.CreateArray(
                new[] { "Null" }.Select(
                        x => new SCLConstant<StringStream>(new StringStream(x)) as
                            IStep<StringStream>
                    )
                    .ToList()
            )
        );

    /// <summary>
    /// Whether transformations are case sensitive
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("False")]
    public IStep<SCLBool> CaseSensitive { get; set; } = new SCLConstant<SCLBool>(SCLBool.False);

    /// <summary>
    /// Whether additional properties outside the schema should be removed
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("True if the Schema does not allow extra properties")]
    public IStep<SCLBool>? RemoveExtraProperties { get; set; } = null;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<Transform, Array<Entity>>();
}
