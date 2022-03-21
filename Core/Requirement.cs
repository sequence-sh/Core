using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Reductech.Sequence.Core;

/// <summary>
/// A requirement of a step or step property
/// </summary>
[DataContract]
[JsonConverter(typeof(RequirementJsonConverter))]
public abstract record Requirement([property: DataMember] string ConnectorName) : IEntityConvertible
{
    /// <summary>
    /// Check that the requirement is met
    /// </summary>
    /// <param name="stepFactoryStore"></param>
    /// <returns></returns>
    public Result<Unit, IErrorBuilder> Check(StepFactoryStore stepFactoryStore)
    {
        var connectorSettings = stepFactoryStore.ConnectorData.Select(x => x.ConnectorSettings)
            .FirstOrDefault(x => x.Id.Equals(ConnectorName, StringComparison.OrdinalIgnoreCase));

        if (connectorSettings is null)
            return ErrorCode.MissingStepSettings.ToErrorBuilder(ConnectorName);

        var r = Check(connectorSettings);

        return r;
    }

    /// <summary>
    /// Check that this requirement is met by these connector settings
    /// </summary>
    protected abstract Result<Unit, IErrorBuilder> Check(ConnectorSettings connectorSettings);

    /// <summary>
    /// Get the text of this requirement
    /// </summary>
    public abstract string GetText();

    /// <inheritdoc />
    public sealed override string ToString() => GetText();

    /// <summary>
    /// Format the Connector Name.
    /// Get the section after the last period
    /// </summary>
    protected string FormattedConnectorName()
    {
        return ConnectorName.Split('.').Last();
    }

    /// <summary>
    /// Group requirements and remove redundant ones.
    /// </summary>
    public static IEnumerable<Requirement> CompressRequirements(
        IReadOnlyCollection<Requirement> requirements)
    {
        foreach (var connectorRequirement in requirements
                     .OfType<ConnectorRequirement>()
                     .GroupBy(x => x.ConnectorName, StringComparer.OrdinalIgnoreCase)
                )
        {
            yield return connectorRequirement.First();
        }

        foreach (var featureGroup in requirements
                     .OfType<FeatureRequirement>()
                     .GroupBy(
                         x => x.ConnectorName + x.FeaturesKey,
                         StringComparer.OrdinalIgnoreCase
                     )
                )
        {
            var features = featureGroup.SelectMany(x => x.RequiredFeatures)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToImmutableList();

            yield return featureGroup.First() with { RequiredFeatures = features };
        }

        foreach (var group in requirements
                     .OfType<VersionRequirement>()
                     .GroupBy(x => x.ConnectorName + x.VersionKey, StringComparer.OrdinalIgnoreCase)
                )
        {
            var highestMaxVersion = group.Select(x => x.MaxVersion).Max();
            var lowestMinVersion  = group.Select(x => x.MinVersion).Min();
            var notes             = group.Select(x => x.Notes).WhereNotNull().Distinct().ToList();
            var text              = notes.Any() ? string.Join("; ", notes) : null;

            yield return group.First() with
            {
                MaxVersion = highestMaxVersion, MinVersion = lowestMinVersion, Notes = text
            };
        }
    }
}

/// <summary>
/// Requires a particular connector
/// </summary>
[DataContract]
public sealed record ConnectorRequirement
    (string ConnectorName) : Requirement(ConnectorName)
{
    /// <inheritdoc />
    protected override Result<Unit, IErrorBuilder> Check(ConnectorSettings connectorSettings)
    {
        return Unit.Default;
    }

    /// <inheritdoc />
    public override string GetText()
    {
        return ConnectorName; //Don't use FormattedConnectorName
    }
}

/// <summary>
/// Requires a particular version of some piece of software (not a connector)
/// </summary>
[DataContract]
public sealed record VersionRequirement
(
    string ConnectorName,
    [property: DataMember] string VersionKey,
    [property: DataMember] Version? MinVersion = null,
    [property: DataMember] Version? MaxVersion = null,
    [property: DataMember] string? Notes = null) : Requirement(ConnectorName)
{
    /// <inheritdoc />
    protected override Result<Unit, IErrorBuilder> Check(ConnectorSettings connectorSettings)
    {
        if (connectorSettings.Settings is null ||
            !connectorSettings.Settings.TryGetValue(VersionKey, out var v) ||
            !Version.TryParse(v.ToString(), out var version))
            return ErrorCode.MissingStepSettingsValue.ToErrorBuilder(ConnectorName, VersionKey);

        if (MaxVersion != null && MaxVersion < version)
            return ErrorCode.RequirementNotMet.ToErrorBuilder(this);

        if (MinVersion != null && MinVersion > version)
            return ErrorCode.RequirementNotMet.ToErrorBuilder(this);

        return Unit.Default;
    }

    /// <inheritdoc />
    public override string GetText()
    {
        if (MinVersion is not null)
        {
            if (MaxVersion is not null)
                return $"{FormattedConnectorName()}.{VersionKey} {MinVersion}-{MaxVersion}";

            return $"{FormattedConnectorName()}.{VersionKey} {MinVersion}";
        }

        if (MaxVersion is not null)
        {
            return $"{FormattedConnectorName()}.{VersionKey} <= {MaxVersion}";
        }

        return $"{FormattedConnectorName()}.{VersionKey}";
    }
}

/// <summary>
/// Requires a particular feature of some software
/// </summary>
[DataContract]
public sealed record FeatureRequirement(
        string ConnectorName,
        [property: DataMember] string FeaturesKey,
        [property: DataMember] IReadOnlyList<string> RequiredFeatures)
    : Requirement(ConnectorName)
{
    /// <summary>
    /// Get the text of this requirement
    /// </summary>
    public override string GetText()
    {
        return $"{FormattedConnectorName()}.{FeaturesKey}: {string.Join(", ", RequiredFeatures)}";
    }

    /// <inheritdoc />
    protected override Result<Unit, IErrorBuilder> Check(ConnectorSettings connectorSettings)
    {
        if (connectorSettings.Settings is null ||
            !connectorSettings.Settings.TryGetValue(FeaturesKey, out var features))
            return ErrorCode.MissingStepSettingsValue.ToErrorBuilder(ConnectorName, FeaturesKey);

        var featuresEntity = ISCLObject.CreateFromCSharpObject(features);

        List<string> actualFeatures;

        if (featuresEntity is IArray nestedList)
            actualFeatures = nestedList.ListIfEvaluated()
                .Value.Select(x => x.Serialize(SerializeOptions.Primitive))
                .ToList();
        else
        {
            actualFeatures =
                new List<string> { featuresEntity.Serialize(SerializeOptions.Primitive) };
        }

        var missingFeatures = RequiredFeatures.Except(
            actualFeatures,
            StringComparer.OrdinalIgnoreCase
        );

        if (missingFeatures.Any())
        {
            return ErrorCode.RequirementNotMet.ToErrorBuilder(this);
        }

        return Unit.Default;
    }
}
