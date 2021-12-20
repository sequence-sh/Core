using System.Runtime.Serialization;
using System.Text.Json;
using Reductech.Sequence.ConnectorManagement.Base;

namespace Reductech.Sequence.Core.Tests;

[UseTestOutputHelper]
public partial class TryGetSettingsTest
{
    private const string SettingsText =
        @"
{
    ""Connectors"": {
        ""Reductech.Sequence.Connectors.MyConnector"": {
            ""Id"": ""Reductech.Sequence.Connectors.MyConnector"",
            ""Version"": ""0.13.0"",

            ""Settings"": {
                ""Username"": ""MrUser"",
                ""Password"": ""MrPassword"",
                ""Url"": ""http://MyUrl/""
            }


        }
    }
}
    ";

    private const string ConnectorKey = "Reductech.Sequence.Connectors.MyConnector";

    [Fact]
    public void ShouldGetSettings()
    {
        var settingsObject = JsonSerializer.Deserialize<MyConnectorSettings>(
            SettingsText,
            new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                Converters = { EntityJsonConverter.Instance, VersionJsonConverter.Instance }
            }
        );

        var settingsEntity = (Entity)ISCLObject.CreateFromCSharpObject(settingsObject);

        var mySettings = TryGetMySettings(settingsEntity);

        mySettings.ShouldBeSuccessful();

        mySettings.Value.Username.Should().Be("MrUser");
        mySettings.Value.Password.Should().Be("MrPassword");
        mySettings.Value.Url.Should().Be("http://MyUrl/");
    }

    /// <summary>
    /// Try to get a TesseractSettings from a list of Connector Informations
    /// </summary>
    private static Result<MySettings, IErrorBuilder> TryGetMySettings(Entity settings)
    {
        var connectorEntityValue = settings.TryGetValue(
            new EntityPropertyKey(
                StateMonad.ConnectorsKey,
                ConnectorKey,
                "Settings"
            )
        );

        if (connectorEntityValue.HasNoValue ||
            connectorEntityValue.Value is not Entity nestedEntity)
            return ErrorCode.MissingStepSettings.ToErrorBuilder(ConnectorKey);

        var connectorSettings =
            EntityConversionHelpers.TryCreateFromEntity<MySettings>(nestedEntity);

        return connectorSettings;
    }

    [DataContract]
    public sealed class MyConnectorSettings : IEntityConvertible
    {
        [DataMember] public Dictionary<string, ConnectorSettings> Connectors { get; set; }
    }

    [DataContract]
    public sealed class MySettings : IEntityConvertible
    {
        [DataMember] public string Username { get; set; } = null!;

        [DataMember] public string Password { get; set; } = null!;

        [DataMember] public string Url { get; set; } = null!;
    }
}
