using Sequence.Core.Internal.Documentation;

namespace Sequence.Core.Steps;

/// <summary>
/// Generates documentation for all available steps.
/// </summary>
[SCLExample(
    @"- <root> = 'sequence/steps'
- (DocumentationCreate)['AllPages'] | ForEach (
    - <path> = $""{<root>}/{<>['Directory']}/{<>['FileName']}""
    - log <path>
)",
    Description = "Logs all the file paths",
    ExecuteInTests = false
)]
[SCLExample("GenerateDocumentation | EntityFormat", ExampleOutput, ExecuteInTests = false)]
[SCLExample(
    "log (stringContains (DocumentationCreate RootUrl: 'MyRoot').MainContents.FileText 'MyRoot/Core/And.md')",
    expectedLogs: "True"
)]
[Alias("DocGen")]
[Alias("GenerateDocumentation")]
[TypeReferenceSchema(
    @"{
  ""title"": ""Documentation"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""properties"": {
    ""MainContents"": {
      ""type"": ""object"",
      ""additionalItems"": {
        ""type"": ""object""
      },
      ""required"": [
        ""PageType"",
        ""FileName"",
        ""Title"",
        ""FileText"",
        ""Directory""
      ],
      ""properties"": {
        ""PageType"": {
          ""type"": ""string""
        },
        ""FileName"": {
          ""type"": ""string""
        },
        ""Title"": {
          ""type"": ""string""
        },
        ""FileText"": {
          ""type"": ""string""
        },
        ""Directory"": {
          ""type"": ""string""
        }
      }
    },
    ""Categories"": {
      ""type"": ""array"",
      ""items"": {
        ""type"": ""object""
      },
      ""prefixItems"": []
    },
    ""Enums"": {
      ""type"": ""array"",
      ""items"": {
        ""type"": ""object""
      },
      ""prefixItems"": []
    },
    ""AllPages"": {
      ""type"": ""array"",
      ""items"": {
        ""type"": ""object""
      },
      ""prefixItems"": []
    }
  },
  ""required"": [
    ""MainContents"",
    ""Categories"",
    ""Enums"",
    ""AllPages""
  ]
}"
)]
public sealed class DocumentationCreate : CompoundStep<Entity>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Entity, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await RootUrl.WrapStringStream().Run(stateMonad, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<Entity>();

        var documented = stateMonad.StepFactoryStore
            .Dictionary
            .GroupBy(x => x.Value, x => x.Key)
            .Select(x => new StepWrapper(x))
            .ToList();

        var creationResult = DocumentationCreator.CreateDocumentation(
            documented,
            new DocumentationOptions() { RootUrl = r.Value } //Use default values
        );

        return creationResult.ConvertToEntity();
    }

    /// <summary>
    /// Root URL of all links in the documentation. You should include a slash `/` at the end.
    /// </summary>
    [StepProperty(1)]
    [DefaultValueExplanation("Empty String")]
    public IStep<StringStream> RootUrl { get; set; } = new SCLConstant<StringStream>("");

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<DocumentationCreate, Entity>();

    const string ExampleOutput =
        @"(
	'MainContents': (
		'PageType': ""Contents""
		'FileName': ""all.md""
		'Title': ""all""
		'FileText': ""...""
		'Directory': null
	)
  'Categories': [
    (
      'CategoryContents': (
        'Category': ""Core""
        'PageType': ""Contents""
        'FileName': ""Core.md""
        'Title': ""Core""
        'FileText': ""...""
        'Directory': null
      )
      'Steps': [
				(
					'Category': ""Core""
					'StepName': ""And""
					'Aliases': [""And""]
					'Summary': null
					'ReturnType': ""Boolean""
					'StepParameters': [
						(
							'Name': ""Terms""
							'Type': ""Array<bool>""
							'Summary': null
							'Required': True
							'Aliases': null
						)
					]
					'PageType': ""Step""
					'FileName': ""And.md""
					'Title': ""And""
					'FileText': ""..""
					'Directory': ""Core""
				),
        ...
      ]
    ),
    ...
  ]
  'Enums': [
    (
      'Values': [
        ('Name': ""Default"" Summary: ""The default encoding.""),
        ('Name': ""Ascii"" Summary: ""Ascii""),
        ('Name': ""BigEndianUnicode"" Summary: ""Unicode with big-endian byte order""),
        ('Name': ""UTF8"" Summary: ""UTF8 with no byte order mark.""),
        ('Name': ""UTF8BOM"" Summary: ""UTF8 with byte order mark.""),
        ('Name': ""UTF32"" Summary: ""UTF32""),
        ('Name': ""Unicode"" Summary: ""Unicode with little-endian byte order"")
      ]
      'PageType': ""Enums""
      'FileName': ""EncodingEnum.md""
      'Title': ""EncodingEnum""
      'FileText': """"
      Directory: """"
    ),
    ...
  ]
  'AllPages': [
    (
      'PageType': ""Contents""
      'FileName': ""all.md""
      'Title': ""all""
      'FileText': ""...""
      'Directory': null
    ),
    (
      'Category': ""Core""
      'PageType': ""Contents""
      'FileName': ""Core.md""
      'Title': ""Core""
      'FileText': ""...""
      'Directory': null
    ),
    ...
  ]
)
";
}
