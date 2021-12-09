using Microsoft.Extensions.Logging.Abstractions;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class EntityFormatTests : StepTestBase<EntityFormat, StringStream>
{
    [Fact]
    public async Task FormatEntityShouldRoundTrip()
    {
        var entity = Entity.Create(
            ("listProp1", new List<int>()
            {
                1,
                2,
                3,
                4,
                5
            }),
            ("listProp2", new List<Entity>()
            {
                Entity.Create(
                    ("prop1", 123),
                    ("prop2", "abc")
                ),
                Entity.Create(
                    ("prop1", 456),
                    ("prop2", "def")
                ),
            }),
            ("listProp3",
             new List<List<int>>() { new() { 1, 2, 3 }, new() { 4, 5, 6 }, new() { 7, 8, 9 }, })
        );

        var formatted = entity.Format();

        TestOutputHelper.WriteLine(formatted);

        var stepFactoryStore = StepFactoryStore.Create();

        var parseResult = SCLParsing.TryParseStep(formatted)
            .Bind(
                x => x.TryFreeze(
                    SCLRunner.RootCallerMetadata,
                    stepFactoryStore
                )
            );

        parseResult.ShouldBeSuccessful();

        parseResult.Value.Should()
            .BeOfType<CreateEntityStep>();

        var runResult = await
            parseResult.Value.Run<Entity>(
                new StateMonad(
                    NullLogger.Instance,
                    stepFactoryStore,
                    null!,
                    new Dictionary<string, object>()
                ),
                CancellationToken.None
            );

        runResult.ShouldBeSuccessful();

        runResult.Value.Serialize().Should().Be(entity.Serialize());
    }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Format Simple Entity",
                new Log<StringStream>()
                {
                    Value = new EntityFormat()
                    {
                        Entity = Constant(
                            Entity.Create(
                                ("prop1", 123),
                                ("prop2", "abc")
                            )
                        )
                    }
                },
                Unit.Default,
                @"(
    'prop1': 123
    'prop2': ""abc""
)
"
            );

            yield return new StepCase(
                "Format Complex entity",
                new Log<StringStream>()
                {
                    Value = new EntityFormat()
                    {
                        Entity = Constant(
                            Entity.Create(
                                ("listProp1", new List<int>()
                                {
                                    1,
                                    2,
                                    3,
                                    4,
                                    5
                                }),
                                ("listProp2", new List<Entity>()
                                {
                                    Entity.Create(
                                        ("prop1", 123),
                                        ("prop2", "abc")
                                    ),
                                    Entity.Create(
                                        ("prop1", 456),
                                        ("prop2", "def")
                                    ),
                                }),
                                ("listProp3",
                                 new List<List<int>>()
                                 {
                                     new() { 1, 2, 3 }, new() { 4, 5, 6 }, new() { 7, 8, 9 },
                                 })
                            )
                        )
                    }
                },
                Unit.Default,
                @"(
	'listProp1': [1, 2, 3, 4, 5]
	'listProp2': [
		(
			'prop1': 123
			'prop2': ""abc""
		),
		(
			'prop1': 456
			'prop2': ""def""
		)
	]
	'listProp3': [
		[1, 2, 3],
		[4, 5, 6],
		[7, 8, 9]
	]
)
"
            );
        }
    }
}
