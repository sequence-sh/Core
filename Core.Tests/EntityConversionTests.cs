using System.Collections.Immutable;
using Sequence.ConnectorManagement.Base;
using Sequence.Core.Internal.Parser;

namespace Sequence.Core.Tests;

public class EntityConversionTests
{
    private const string ConnectorName = "Sequence.Core.Tests";

    private static readonly Configuration TestConfiguration = new()
    {
        DoNotSplit        = true,
        Priority          = 3,
        TargetMachineTags = new List<string>() { "alpha", "beta" },
        AdditionalRequirements = new List<Requirement>()
        {
            new FeatureRequirement(
                ConnectorName,
                "Features",
                new List<string> { "Apple", "Banana" }
            ),
            new VersionRequirement(
                ConnectorName,
                "Version",
                new Version(1, 2, 3, 4),
                new Version(5, 6, 7, 8)
            )
        }
    };

    private static readonly Configuration TestConfiguration2 = new()
    {
        DoNotSplit             = true,
        Priority               = 3,
        TargetMachineTags      = new List<string> { "alpha", "beta" },
        AdditionalRequirements = new List<Requirement>()
    };

    private const string TestConfigurationString =
        "('AdditionalRequirements': [('FeaturesKey': \"Features\" 'RequiredFeatures': [\"Apple\", \"Banana\"] 'ConnectorName': \"Sequence.Core.Tests\"), ('VersionKey': \"Version\" 'MinVersion': \"1.2.3.4\" 'MaxVersion': \"5.6.7.8\" 'ConnectorName': \"Sequence.Core.Tests\")] 'TargetMachineTags': [\"alpha\", \"beta\"] 'DoNotSplit': True 'Priority': 3)";

    private const string TestConfigurationString2 =
        "('AdditionalRequirements': [] 'TargetMachineTags': [\"alpha\", \"beta\"] 'DoNotSplit': True 'Priority': 3)";

    [Fact]
    public void ConfigurationShouldConvertToEntityCorrectly()
    {
        var entity = TestConfiguration.ConvertToEntity();

        var s = entity.Serialize(SerializeOptions.Serialize);

        s.Should().Be(TestConfigurationString);

        var config = EntityConversionHelpers.TryCreateFromEntity<Configuration>(entity);
        config.ShouldBeSuccessful();

        var cvEntity = config.Value.ConvertToEntity();

        cvEntity.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public void ConfigurationShouldConvertToEntityCorrectly2()
    {
        var entity = TestConfiguration2.ConvertToEntity();

        var s = entity.Serialize(SerializeOptions.Serialize);

        s.Should().Be(TestConfigurationString2);

        var config = EntityConversionHelpers.TryCreateFromEntity<Configuration>(entity);
        config.ShouldBeSuccessful();
    }

    [Fact]
    public void CreateFromDictionaryShouldSetOrderCorrectly()
    {
        var dict = new Dictionary<string, object>() { { "Foo", 1 }, { "Bar", "Two" } };

        var entity = Entity.Create(dict);

        var expected = Entity.Create(("Foo", 1), ("Bar", "Two"));

        entity.Should().BeEquivalentTo(expected);
    }

    private static Entity CreateEntityFromString(string s)
    {
        var sfs = StepFactoryStore.Create();

        var parseResult =
            SCLParsing.TryParseStep(s)
                .Bind(
                    x => x.TryFreeze(
                        new CallerMetadata(
                            nameof(EntityConversionTests),
                            nameof(Entity),
                            TypeReference.Entity.NoSchema
                        ),
                        sfs,
                        OptimizationSettings.None
                    )
                )
                .Map(
                    x => x.TryGetConstantValueAsync(
                        ImmutableDictionary<VariableName, ISCLObject>.Empty,
                        sfs
                    )
                );

        if (!parseResult.Value.IsCompleted)
        {
            throw new Exception("Parse result should complete immediately");
        }

        parseResult.ShouldBeSuccessful();

        var sclObject = parseResult.Value.Result.GetValueOrThrow();

        if (sclObject is Entity entity)
            return entity;

        throw new Exception("Parse result should be an entity");
    }

    [Fact]
    public void EntityShouldConvertToConfigurationCorrectly()
    {
        var entity = CreateEntityFromString(TestConfigurationString2);

        var configurationResult =
            EntityConversionHelpers.TryCreateFromEntity<Configuration>(entity);

        configurationResult.ShouldBeSuccessful();

        var conf = configurationResult.Value;

        conf.Should().Be(TestConfiguration2);
    }

    [Fact]
    public void ConvertToEntityShouldConvertNestedEntities()
    {
        var settings = new ConnectorSettings
        {
            Enable   = true,
            Id       = "Ultimate",
            Version  = new Version(3, 1).ToString(),
            Settings = new Dictionary<string, object> { { "a", 1 }, { "b", 2 } }
        };

        var entity = EntityConversionHelpers.ConvertToEntity(settings);

        var s = entity.Serialize(SerializeOptions.Serialize);

        s.Should()
            .Be(
                @"('Id': ""Ultimate"" 'Version': ""3.1"" 'Enable': True 'Settings': ('a': 1 'b': 2))"
            );
    }

    [Fact]
    public void ShortShouldConvertCorrectly()
    {
        var e = Entity.Create(("short", Convert.ToInt16(11)));

        e.TryGetValue("short").ShouldHaveValue();
        e.TryGetValue("short").GetValueOrThrow().Should().Be(new SCLInt(11));
    }

    [Fact]
    public void EnumerationShouldConvertCorrectly()
    {
        var e = Entity.Create(("enumeration", new SCLEnum<TextCase>(TextCase.Upper)));

        e.TryGetValue("enumeration").ShouldHaveValue();

        e.TryGetValue("enumeration")
            .GetValueOrThrow()
            .Should()
            .Be(new SCLEnum<TextCase>(TextCase.Upper));
    }

    [Fact]
    public void EmptyListShouldConvertCorrectly()
    {
        var e = Entity.Create(("emptyList", new List<int>()));

        e.TryGetValue("emptyList").ShouldHaveValue();

        var listValue =
            e.TryGetValue("emptyList")
                .GetValueOrThrow();

        listValue
            .Should()
            .Be(EagerArray<ISCLObject>.Empty);
    }
}
