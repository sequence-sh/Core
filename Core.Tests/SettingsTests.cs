using System.Collections.Generic;
using FluentAssertions;
using Reductech.EDR.Core.Entities;
using Xunit;

namespace Reductech.EDR.Core.Tests
{

[AutoTheory.UseTestOutputHelper]
public partial class SettingsTests
{
    [Fact]
    public void TestCreatingSettingsFromString()
    {
        var settings = SCLSettings.CreateFromString(ConnectorJson);

        TestOutputHelper.WriteLine(settings.ToString());

        settings.Entity.TryGetValue("Connectors")
            .Value.AsT7.TryGetValue("Nuix")
            .Value.AsT7.TryGetValue("UseDongle")
            .Value.ToString()
            .Should()
            .Be(true.ToString());

        var udString = settings.Entity.TryGetNestedString("Connectors", "Nuix", "UseDongle");

        udString.HasValue.Should().BeTrue();
        udString.Value.Should().Be(true.ToString());

        var udBool = settings.Entity.TryGetNestedBool("Connectors", "Nuix", "UseDongle");
        udBool.Should().BeTrue();
    }

    [Fact]
    public void TestCreatingSettingsFromDictionary()
    {
        var dict = new Dictionary<string, object>()
        {
            {
                "nuix",
                new Dictionary<string, object>()
                {
                    { "UseDongle", true }, { "Features", new List<string>() { "a", "b", "c" } }
                }
            }
        };

        var entity = Entity.Create(("Connectors", dict));

        var settings = new SCLSettings(entity);

        TestOutputHelper.WriteLine(settings.ToString());

        settings.Entity.TryGetValue("Connectors")
            .Value.AsT7.TryGetValue("Nuix")
            .Value.AsT7.TryGetValue("UseDongle")
            .Value.ToString()
            .Should()
            .Be(true.ToString());

        var useDongleString = settings.Entity.TryGetNestedString("Connectors", "Nuix", "UseDongle");

        useDongleString.HasValue.Should().BeTrue();

        useDongleString.Value.Should().Be(true.ToString());

        var useDongleBool = settings.Entity.TryGetNestedBool("Connectors", "Nuix", "UseDongle");
        useDongleBool.Should().BeTrue();

        var featuresList = settings.Entity.TryGetNestedList("Connectors", "Nuix", "Features");

        featuresList.HasValue.Should().BeTrue();

        featuresList.Value.Should().BeEquivalentTo("a", "b", "c");
    }

    private const string ConnectorJson =
        @"{
  ""connectors"": {
    ""nuix"": {
      ""useDongle"": true,
      ""exeConsolePath"": ""C:\\Program Files\\Nuix\\Nuix 8.8\\nuix_console.exe"",
      ""version"": ""8.8"",
      ""features"": [
        ""ANALYSIS"",
        ""CASE_CREATION"",
        ""EXPORT_ITEMS"",
        ""METADATA_IMPORT"",
        ""OCR_PROCESSING"",
        ""PRODUCTION_SET""
      ]
    }
  },
  ""nlog"": {
    ""throwConfigExceptions"": true,
    ""targets"": {
      ""fileTarget"": {
        ""type"": ""File"",
        ""fileName"": ""${basedir:fixtempdir=true}\\edr.log"",
        ""layout"": ""${date} ${level:uppercase=true} ${message} ${exception}""
      },
      ""consoleInfo"": {
        ""type"": ""Console"",
        ""layout"": ""${date} ${message}""
      },
      ""consoleError"": {
        ""type"": ""Console"",
        ""layout"": ""${date} ${level:uppercase=true} ${message}"",
        ""error"": true
      }
    },
    ""rules"": [
      {
        ""logger"": ""*"",
        ""minLevel"": ""Error"",
        ""writeTo"": ""fileTarget,consoleError"",
        ""final"": true
      },
      {
        ""logger"": ""*"",
        ""minLevel"": ""Trace"",
        ""writeTo"": ""fileTarget,consoleInfo""
      }
    ]
  }
}
";
}

}
