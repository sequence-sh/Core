using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using Reductech.EDR.ConnectorManagement.Base;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core
{

/// <summary>
/// A requirement of a step or step property
/// </summary>
[DataContract]
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
            .GroupBy(x => x.ConnectorName + x.FeaturesKey, StringComparer.OrdinalIgnoreCase)
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
    public override string ToString()
    {
        return ConnectorName;
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
    public override string ToString()
    {
        if (MinVersion is not null)
        {
            if (MaxVersion is not null)
                return $"{ConnectorName}.{VersionKey} {MinVersion}-{MaxVersion}";

            return $"{ConnectorName}.{VersionKey} {MinVersion}";
        }

        if (MaxVersion is not null)
        {
            return $"{ConnectorName}.{VersionKey} <= {MaxVersion}";
        }

        return $"{ConnectorName}.{VersionKey}";
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
    /// <inheritdoc />
    public override string ToString()
    {
        return $"{ConnectorName}.{FeaturesKey}: {string.Join(", ", RequiredFeatures)}";
    }

    /// <inheritdoc />
    protected override Result<Unit, IErrorBuilder> Check(ConnectorSettings connectorSettings)
    {
        if (connectorSettings.Settings is null ||
            !connectorSettings.Settings.TryGetValue(FeaturesKey, out var features))
            return ErrorCode.MissingStepSettingsValue.ToErrorBuilder(ConnectorName, FeaturesKey);

        var featuresEntity = EntityValue.CreateFromObject(features);

        List<string> actualFeatures;

        if (featuresEntity is EntityValue.NestedList nestedList)
            actualFeatures = nestedList.Value.Select(GetEntityValueString).ToList();
        else
        {
            actualFeatures = new List<string> { GetEntityValueString(featuresEntity) };
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

        static string GetEntityValueString(EntityValue entityValue)
        {
            if (entityValue is EntityValue.EnumerationValue ev)
            {
                return ev.Value.Value;
            }

            return entityValue.GetPrimitiveString();
        }
    }
}

}
