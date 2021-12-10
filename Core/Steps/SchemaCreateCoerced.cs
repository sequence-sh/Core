namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Create a new schema by analysing the entity properties and values in
/// an array or an entity stream.
/// This Step is best used with flat data sources such as Concordance or
/// CSV as it does additional processing to infer the data types of strings.
/// </summary>
[Alias("GenerateSchemaCoerced")]
[SCLExample(
    "SchemaCreateCoerced Entities: [('StringProperty': \"abc\" 'IntegerProperty': \"123\")] SchemaName: 'My Schema'",
    "('title': \"My Schema\" 'type': \"object\" 'additionalProperties': False 'properties': ('StringProperty': ('type': \"string\") 'IntegerProperty': ('type': \"integer\")) 'required': [\"StringProperty\", \"IntegerProperty\"])",
    "Even though IntegerProperty is represented as a string in the input entity (quoted), it is converted to an integer in the schema."
)]
public sealed class SchemaCreateCoerced : CompoundStep<Entity>
{
    /// <inheritdoc />
    protected override async Task<Result<Entity, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var inputFormatMap = StepMaps.OneOf(
            StepMaps.String(),
            StepMaps.Array(StepMaps.String()),
            StepMaps.DoNothing<Entity>()
        );

        var stepResult = await stateMonad.RunStepsAsync(
            Entities.WrapArray(),
            SchemaName.WrapStringStream(),
            AllowExtraProperties,
            DateInputFormats.WrapNullable(inputFormatMap),
            BooleanTrueFormats.WrapNullable(inputFormatMap),
            BooleanFalseFormats.WrapNullable(inputFormatMap),
            NullFormats.WrapNullable(inputFormatMap),
            CaseSensitive,
            cancellationToken
        );

        if (stepResult.IsFailure)
            return stepResult.ConvertFailure<Entity>();

        var (entities,
            schemaName,
            allowExtraProperties,
            dateInputFormats,
            boolTrueFormats,
            boolFalseFormats,
            nullFormats,
            caseSensitive
            ) = stepResult.Value;

        var sco = new SchemaConversionOptions(
            Formatter.Create(dateInputFormats),
            Formatter.Create(boolTrueFormats),
            Formatter.Create(boolFalseFormats),
            Formatter.Create(nullFormats),
            caseSensitive
        );

        var schema =
            entities.Select(x => new EntityValue.NestedEntity(x).ToSchemaNode("", sco))
                .Aggregate((a, b) => a.Combine(b))
                .ToJsonSchema();

        var jsonSchemaBuilder = new JsonSchemaBuilder()
            .Title(schemaName)
            .Type(SchemaValueType.Object)
            .AdditionalProperties(allowExtraProperties ? JsonSchema.True : JsonSchema.False);

        var props    = schema.Keywords?.OfType<PropertiesKeyword>().FirstOrDefault();
        var required = schema.Keywords?.OfType<RequiredKeyword>().FirstOrDefault();

        if (props is not null)
            jsonSchemaBuilder.Properties(props.Properties);

        if (required is not null)
            jsonSchemaBuilder.Required(required.Properties);

        var schemaEntity = Entity.Create(jsonSchemaBuilder.Build().ToJsonDocument().RootElement);
        return schemaEntity;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<SchemaCreateCoerced, Entity>();

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
    public IStep<SCLBool> AllowExtraProperties { get; set; } = new BoolConstant(SCLBool.False);

    /// <summary>
    /// ISO 8601 Date Formats to use for strings representing dates
    /// This can either be a string, and array of string, or an entity mapping field names to strings or arrays of string
    /// </summary>
    [StepProperty()]
    [DefaultValueExplanation("yyyy-MM-ddTHH:mm:ssK")]
    public IStep<SCLOneOf<StringStream, Array<StringStream>, Entity>>? DateInputFormats
    {
        get;
        set;
    } =
        new OneOfStep<StringStream, Array<StringStream>, Entity>(
            new StringConstant("yyyy-MM-ddTHH:mm:ssK")
        );

    /// <summary>
    /// Strings which represent truth.
    /// This can either be a string, and array of string, or an entity mapping field names to strings or arrays of string
    /// </summary>
    [StepProperty()]
    [DefaultValueExplanation("True")]
    public IStep<SCLOneOf<StringStream, Array<StringStream>, Entity>>? BooleanTrueFormats
    {
        get;
        set;
    }
        = new OneOfStep<StringStream, Array<StringStream>, Entity>(
            ArrayNew<StringStream>.CreateArray(
                new[] { "True" }.Select(
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
    [DefaultValueExplanation("False")]
    public IStep<OneOf<StringStream, Array<StringStream>, Entity>>? BooleanFalseFormats
    {
        get;
        set;
    }
        = new OneOfStep<StringStream, Array<StringStream>, Entity>(
            ArrayNew<StringStream>.CreateArray(
                new[] { "False" }.Select(
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
    [DefaultValueExplanation("No null values")]
    public IStep<OneOf<StringStream, Array<StringStream>, Entity>>? NullFormats { get; set; } =
        null;

    /// <summary>
    /// Whether transformations are case sensitive
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("False")]
    public IStep<SCLBool> CaseSensitive { get; set; } = new BoolConstant(SCLBool.False);
}
