using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Reductech.EDR.Core
{

/// <summary>
/// Json converter for requirement
/// </summary>
public class RequirementConverter : JsonConverter<Requirement>
{
    private RequirementConverter() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static JsonConverter<Requirement> Instance { get; } = new RequirementConverter();

    private class MutableRequirement
    {
        public static MutableRequirement Create(Requirement r)
        {
            return r switch
            {
                ConnectorRequirement connectorRequirement => new MutableRequirement()
                {
                    ConnectorName = connectorRequirement.ConnectorName
                },
                FeatureRequirement featureRequirement => new MutableRequirement()
                {
                    ConnectorName    = featureRequirement.ConnectorName,
                    FeaturesKey      = featureRequirement.FeaturesKey,
                    RequiredFeatures = featureRequirement.RequiredFeatures
                },
                VersionRequirement versionRequirement => new MutableRequirement()
                {
                    ConnectorName = versionRequirement.ConnectorName,
                    VersionKey    = versionRequirement.VersionKey,
                    MaxVersion    = versionRequirement.MaxVersion,
                    MinVersion    = versionRequirement.MinVersion,
                    Notes         = versionRequirement.Notes
                },
                _ => throw new ArgumentOutOfRangeException(nameof(r))
            };
        }

        public Requirement ToRequirement()
        {
            if (VersionKey is not null)
                return new VersionRequirement(
                    ConnectorName,
                    VersionKey,
                    MinVersion,
                    MaxVersion
                );

            if (FeaturesKey is not null)
                return new FeatureRequirement(
                    ConnectorName,
                    FeaturesKey,
                    RequiredFeatures ?? new List<string>()
                );

            return new ConnectorRequirement(ConnectorName);
        }

        public string ConnectorName { get; set; }

        public string? VersionKey { get; set; }
        public Version? MinVersion { get; set; }
        public Version? MaxVersion { get; set; }
        public string? Notes { get; set; }

        public string? FeaturesKey { get; set; }
        public IReadOnlyList<string>? RequiredFeatures { get; set; }
    }

    /// <inheritdoc />
    public override void WriteJson(
        JsonWriter writer,
        Requirement? value,
        JsonSerializer serializer)
    {
        if (value == null)
            writer.WriteNull();
        else
            serializer.Serialize(writer, MutableRequirement.Create(value));
    }

    /// <inheritdoc />
    public override Requirement ReadJson(
        JsonReader reader,
        Type objectType,
        Requirement? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var mutableRequirement = serializer.Deserialize<MutableRequirement>(reader)!;
        return mutableRequirement.ToRequirement();
    }
}

}
