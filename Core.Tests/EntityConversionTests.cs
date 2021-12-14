using Reductech.EDR.ConnectorManagement.Base;
using Reductech.EDR.Core.Internal.Parser;

namespace Reductech.EDR.Core.Tests;

public class EntityConversionTests
{
    private const string ConnectorName = "Reductech.EDR.Core.Tests";

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
        "('AdditionalRequirements': [('FeaturesKey': \"Features\" 'RequiredFeatures': [\"Apple\", \"Banana\"] 'ConnectorName': \"Reductech.EDR.Core.Tests\"), ('VersionKey': \"Version\" 'MinVersion': \"1.2.3.4\" 'MaxVersion': \"5.6.7.8\" 'ConnectorName': \"Reductech.EDR.Core.Tests\")] 'TargetMachineTags': [\"alpha\", \"beta\"] 'DoNotSplit': True 'Priority': 3)";

    private const string TestConfigurationString2 =
        "('AdditionalRequirements': [] 'TargetMachineTags': [\"alpha\", \"beta\"] 'DoNotSplit': True 'Priority': 3)";

    [Fact]
    public void ConfigurationShouldConvertToEntityCorrectly()
    {
        var entity = TestConfiguration.ConvertToEntity();

        var s = entity.ToString();

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

        var s = entity.ToString();

        s.Should().Be(TestConfigurationString2);

        var config = EntityConversionHelpers.TryCreateFromEntity<Configuration>(entity);
        config.ShouldBeSuccessful();
    }

    private static Entity CreateEntityFromString(string s)
    {
        var parseResult =
            SCLParsing.TryParseStep(s)
                .Bind(
                    x => x.TryFreeze(
                        new CallerMetadata(
                            nameof(EntityConversionTests),
                            nameof(Entity),
                            TypeReference.Actual.Entity
                        ),
                        StepFactoryStore.Create()
                    )
                )
                .Map(x => x.TryGetConstantValue());

        parseResult.ShouldBeSuccessful();

        return parseResult.Value.GetValueOrThrow() as Entity;
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

        var s = entity.ToString();

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
