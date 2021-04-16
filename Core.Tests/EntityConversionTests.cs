using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.TestHarness;
using Xunit;

namespace Reductech.EDR.Core.Tests
{

public class EntityConversionTests
{
    private static readonly Configuration TestConfiguration = new()
    {
        DoNotSplit        = true,
        Priority          = 3,
        TargetMachineTags = new List<string>() { "alpha", "beta" },
        AdditionalRequirements = new List<Requirement>()
        {
            new()
            {
                Name       = "Test Requirement 1",
                Features   = new List<string> { "Apple", "Banana" },
                Notes      = "Test Notes",
                MinVersion = new Version(1, 2, 3, 4),
                MaxVersion = new Version(5, 6, 7, 8)
            },
            new() { Name = "Test Requirement 2" }
        }
    };

    private const string TestConfigurationString =
        "(AdditionalRequirements: [(Name: \"Test Requirement 1\" MinVersion: \"1.2.3.4\" MaxVersion: \"5.6.7.8\" Notes: \"Test Notes\" Features: [\"Apple\", \"Banana\"]), (Name: \"Test Requirement 2\")] TargetMachineTags: [\"alpha\", \"beta\"] DoNotSplit: True Priority: 3)";

    [Fact]
    public void ConfigurationShouldConvertToEntityCorrectly()
    {
        var entity = TestConfiguration.ConvertToEntity();

        var s = entity.ToString();

        s.Should().Be(TestConfigurationString);
    }

    private static Entity CreateEntityFromString(string s)
    {
        var parseResult =
            SCLParsing.ParseSequence(s)
                .Bind(
                    x => x.TryFreeze(
                        TypeReference.Actual.Entity,
                        StepFactoryStore.CreateFromAssemblies()
                    )
                )
                .Map(x => x.TryConvertToEntityValue());

        parseResult.ShouldBeSuccessful(x => x.AsString);

        return (parseResult.Value.Value as EntityValue.NestedEntity)!.Value; //could throw exception
    }

    [Fact]
    public void EntityShouldConvertToConfigurationCorrectly()
    {
        var entity = CreateEntityFromString(TestConfigurationString);

        var configurationResult =
            EntityConversionHelpers.TryCreateFromEntity<Configuration>(entity);

        configurationResult.ShouldBeSuccessful(x => x.AsString);

        var conf = configurationResult.Value;

        conf.Should().Be(TestConfiguration);
    }
}

}
