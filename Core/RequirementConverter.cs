using System.Text.Json.Serialization;

namespace Reductech.Sequence.Core;

/// <summary>
/// JSONConverter for Requirements
/// </summary>
public class RequirementJsonConverter : JsonConverter<Requirement>
{
    /// <summary>
    /// The Instance
    /// </summary>
    public static JsonConverter<Requirement> Instance { get; } = new RequirementJsonConverter();

    /// <inheritdoc />
    public override Requirement? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var mutableRequirement =
            JsonSerializer.Deserialize<MutableRequirement>(ref reader, options);

        return mutableRequirement?.ToRequirement();
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        Requirement value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, MutableRequirement.Create(value));
    }

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

        /// <summary>
        /// The name of the connector
        /// </summary>
        public string ConnectorName { get; set; } = null!;

        public string? VersionKey { get; set; }
        public Version? MinVersion { get; set; }
        public Version? MaxVersion { get; set; }
        public string? Notes { get; set; }

        public string? FeaturesKey { get; set; }
        public IReadOnlyList<string>? RequiredFeatures { get; set; }
    }
}
