using System.Collections.Generic;
using Reductech.EDR.ConnectorManagement;
using Reductech.EDR.Core.Entities;

namespace Reductech.EDR.Core.Connectors
{

/// <summary>
/// Helpers for converting settings
/// </summary>
public static class ConnectorSettingsHelper
{
    /// <summary>
    /// Create a settings from SCLSettings
    /// </summary>
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
                var convertResult =
                    EntityConversionHelpers.TryCreateFromEntity<ConnectorSettings>(
                        nestedEntity.Value
                    );

                if (convertResult.IsSuccess)
                    yield return new(property.Name, convertResult.Value);
            }
        }
    }
}

}
