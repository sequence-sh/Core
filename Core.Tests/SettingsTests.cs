using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Reductech.EDR.Core.Tests
{

[AutoTheory.UseTestOutputHelper]
public partial class SettingsTests
{
    [Fact]
    public void Test()
    {
        var data = SCLSettings.CreateFromString(ConnectorJson);

        TestOutputHelper.WriteLine(data.ToString());
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
