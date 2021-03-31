using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core
{

/// <summary>
/// A requirement of a step.
/// </summary>
[Serializable]
public sealed class Requirement : IEntityConvertible
{
    /// <summary>
    /// The name of the required software.
    /// </summary>
    [JsonProperty]
    [Required]
    #pragma warning disable 8618
    public string Name { get; set; }
    #pragma warning restore 8618

    /// <summary>
    /// The minimum required version. Inclusive.
    /// </summary>
    [JsonProperty]
    public Version? MinVersion { get; set; }

    /// <summary>
    /// The version above the highest allowed version.
    /// </summary>
    [JsonProperty]
    public Version? MaxVersion { get; set; }

    /// <summary>
    /// Notes on the requirement.
    /// </summary>
    [JsonProperty]
    public string? Notes { get; set; }

    /// <summary>
    /// Required Features
    /// </summary>
    [JsonProperty]
    public IReadOnlyList<string>? Features { get; set; }

    /// <summary>
    /// Check that the requirement is met by these settings.
    /// </summary>
    public Result<Unit, IErrorBuilder> Check(SCLSettings settings)
    {
        if (settings.Entity.Dictionary.TryGetValue(SCLSettings.ConnectorsKey, out var connectors)
         && connectors.BestValue is EntityValue.NestedEntity connectorsEntity)
        {
            var connector = connectorsEntity.Value.TryGetValue(Name);

            if (connector.HasValue && connector.Value is EntityValue.NestedEntity connectorEntity)
            {
                var connectorVersion = connectorEntity.Value.TryGetValue(SCLSettings.VersionKey);

                if (connectorVersion.HasValue)
                {
                    if (Version.TryParse(
                        connectorVersion.Value.GetPrimitiveString(),
                        out var version
                    ))
                    {
                        if (MaxVersion != null && MaxVersion < version)
                            return ErrorCode.RequirementNotMet.ToErrorBuilder(this);

                        if (MinVersion != null && MinVersion > version)
                            return ErrorCode.RequirementNotMet.ToErrorBuilder(this);

                        if (Features != null && Features.Any())
                        {
                            var connectorFeatures =
                                connectorEntity.Value.TryGetValue(SCLSettings.FeaturesKey);

                            if (connectorFeatures.HasValue
                             && connectorFeatures.Value is EntityValue.NestedList featuresList)
                            {
                                var missingFeatures = Features.Except(
                                    featuresList.Value.Select(x => x.GetPrimitiveString()),
                                    StringComparer.OrdinalIgnoreCase
                                );

                                if (missingFeatures.Any())
                                {
                                    return ErrorCode.RequirementNotMet.ToErrorBuilder(this);
                                }
                            }
                            else
                                return ErrorCode.MissingStepSettingsValue.ToErrorBuilder(
                                    Name,
                                    SCLSettings.FeaturesKey
                                );
                        }

                        return Unit.Default;
                    }

                    return ErrorCode.CouldNotParse.ToErrorBuilder(
                        connectorVersion.Value.ToString()!,
                        "Version"
                    );
                }

                return ErrorCode.MissingStepSettingsValue.ToErrorBuilder(
                    Name,
                    SCLSettings.VersionKey
                );
            }
        }
        else { }

        return ErrorCode.MissingStepSettings.ToErrorBuilder(Name);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append(Name);

        if (MinVersion != null)
            sb.Append($" Version {MinVersion}");

        if (MaxVersion != null)
            sb.Append($" Version <= {MaxVersion}");

        if (Notes != null)
            sb.Append($" ({Notes})");

        if (Features != null && Features.Any())
        {
            var features = string.Join(", ", Features);
            sb.Append($" Features: {features}");
        }

        return sb.ToString();
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Requirement r && ToTuple.Equals(r.ToTuple);

    /// <inheritdoc />
    public override int GetHashCode() => ToTuple.GetHashCode();

    private object ToTuple => (Name, MinVersion, MaxVersion, Notes,
                               string.Join(", ", Features ?? new List<string>()));

    /// <summary>
    /// Group requirements and remove redundant ones.
    /// </summary>
    public static IEnumerable<Requirement> CompressRequirements(
        IEnumerable<Requirement> requirements)
    {
        foreach (var group in requirements.GroupBy(x => x.Name.ToLowerInvariant().Trim()))
        {
            if (group.Count() == 1)
                yield return group.Single();

            var highestMaxVersion = group.Select(x => x.MaxVersion).Max();
            var lowestMinVersion  = group.Select(x => x.MinVersion).Min();
            var notes             = group.Select(x => x.Notes).WhereNotNull().Distinct().ToList();
            var text              = notes.Any() ? string.Join("; ", notes) : null;

            var features = group.SelectMany(x => x.Features ?? new List<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            yield return new Requirement
            {
                MaxVersion = highestMaxVersion,
                MinVersion = lowestMinVersion,
                Name       = group.Key,
                Notes      = text,
                Features   = features
            };
        }
    }
}

}
