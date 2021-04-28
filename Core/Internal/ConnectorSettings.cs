using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using Reductech.EDR.Core.Entities;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// Settings for a connector, as found in appsettings.json file
/// </summary>
[Serializable]
public sealed class ConnectorSettings : IEntityConvertible
{
    public static ConnectorSettings DefaultForAssembly(Assembly assembly)
    {
        return new()
        {
            Enable   = true,
            Settings = Entity.Empty,
            Id       = assembly.GetName().Name,
            Version = FileVersionInfo
                .GetVersionInfo(assembly.Location)
                .ProductVersion!
        };
    }

    public static IEnumerable<(string Key, ConnectorSettings Settings)> CreateFromSCLSettings(
        SCLSettings sclSettings)
    {
        var connectorsEntity = sclSettings.Entity.TryGetNestedEntity(SCLSettings.ConnectorsKey);

        if (connectorsEntity.HasNoValue)
            yield break;

        foreach (var property in connectorsEntity.Value)
        {
            if (property.BestValue is EntityValue.NestedEntity nestedEntity)
            {
                var connectorSettings =
                    EntityConversionHelpers.TryCreateFromEntity<ConnectorSettings>(
                        nestedEntity.Value
                    );

                if (connectorSettings.IsSuccess)
                    yield return new(property.Name, connectorSettings.Value);
            }
        }
    }

    /// <summary>
    /// The id of the connector in the registry
    /// </summary>
    [JsonProperty(propertyName: "id")]
    public string Id { get; set; }

    /// <summary>
    /// Whether to enable this connector
    /// </summary>
    [JsonProperty(propertyName: "enable")]
    public bool Enable { get; set; } = true;

    /// <summary>
    /// The version of the connector
    /// </summary>
    [JsonProperty(propertyName: "version")]
    public string Version { get; set; }

    /// <summary>
    /// Individual settings for the connector
    /// </summary>
    [JsonProperty(propertyName: "settings")]
    public Entity Settings { get; set; }

    /// <summary>
    /// String representation
    /// </summary>
    /// <returns></returns>
    public string VersionString() => $"{Id} {Version}";

    /// <inheritdoc />
    public override string ToString()
    {
        return VersionString();
    }
}

}
