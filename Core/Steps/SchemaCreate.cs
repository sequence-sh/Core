namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Create a new schema by analysing the entity properties and values in
/// an array or an entity stream.
/// This Step is best used with data sources where the property values already
/// have a data type (string, date, int...) such as JSON or databases.
/// For generating schemas from flat data sources such as Concordance or
/// CSV, use the `SchemaCreateCoerced` step, which will attempt to infer
/// the property types.
/// </summary>
[Alias("GenerateSchema")]
[Alias("CreateSchema")]
[SCLExample(
    "SchemaCreate Entities: [('StringProperty': \"abc\" 'IntegerProperty': 123)] SchemaName: 'My Schema'",
    "('title': \"My Schema\" 'type': \"object\" 'additionalProperties': False 'properties': ('StringProperty': ('type': \"string\") 'IntegerProperty': ('type': \"integer\")) 'required': [\"StringProperty\", \"IntegerProperty\"])"
)]
[SCLExample(
    "SchemaCreate Entities: [('StringProperty': \"abc\" 'IntegerProperty': \"123\")] SchemaName: 'My Schema'",
    "('title': \"My Schema\" 'type': \"object\" 'additionalProperties': False 'properties': ('StringProperty': ('type': \"string\") 'IntegerProperty': ('type': \"string\")) 'required': [\"StringProperty\", \"IntegerProperty\"])",
    "Since IntegerProperty is quoted, this step interprets it as a string."
)]
public sealed class SchemaCreate : CompoundStep<Entity>
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

        var schema = entities
            .Select(x => x.ToSchemaNode("null", null))
            .Aggregate((a, b) => a.Combine(b))
            .ToJsonSchema();

        var jsonSchemaBuilder = new JsonSchemaBuilder()
            .Title(schemaName)
            .Type(SchemaValueType.Object)
            .AdditionalProperties(allowExtraProperties.Value ? JsonSchema.True : JsonSchema.False);

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
        new SimpleStepFactory<SchemaCreate, Entity>();

    /// <summary>
    /// The array or entity stream to analyse
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("From")]
    public IStep<Array<Entity>> Entities { get; set; } = null!;

    /// <summary>
    /// The name of the schema to create
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("Schema")]
    [Alias("Name")]
    public IStep<StringStream> SchemaName { get; set; } = new SCLConstant<StringStream>("Schema");

    /// <summary>
    /// Whether properties other than the explicitly defined properties are allowed.
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("false")]
    public IStep<SCLBool> AllowExtraProperties { get; set; } =
        new SCLConstant<SCLBool>(SCLBool.False);
}
