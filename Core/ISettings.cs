using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using Newtonsoft.Json;
using Reductech.EDR.Core.Entities;

namespace Reductech.EDR.Core
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// External settings for running the step.
/// </summary>
public record SCLSettings(Entity Entity)
{
    public const string VersionKey = "Version";
    public const string ConnectorsKey = "Connectors";
    public const string FeaturesKey = "Features";

    public static readonly SCLSettings EmptySettings =
        new(new Entity(ImmutableDictionary<string, EntityProperty>.Empty));

    public static SCLSettings CreateFromConfigurationManager()
    {
        var dict = new List<(string key, object? e)>();

        foreach (var key in ConfigurationManager.AppSettings.AllKeys)
        {
            var value = ConfigurationManager.AppSettings[key]!;

            var entity = CreateEntityFromString(value);
            dict.Add((value, entity));
        }

        var newEntity = Entity.Create(dict);

        return new SCLSettings(newEntity);
    }

    public static SCLSettings CreateFromString(string jsonString)
    {
        var entity = CreateEntityFromString(jsonString);
        return new SCLSettings(entity);
    }

    private static Entity CreateEntityFromString(string s)
    {
        var entity = JsonConvert.DeserializeObject<Entity>(s, EntityJsonConverter.Instance)!;
        return entity;
    }
}

}
