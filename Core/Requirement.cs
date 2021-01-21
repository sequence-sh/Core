using Reductech.EDR.Core.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core
{

/// <summary>
/// A requirement of a step.
/// </summary>
public sealed class Requirement
{
    /// <summary>
    /// The name of the required software.
    /// </summary>
    [ConfigProperty(1)]
    [Required]
    #pragma warning disable 8618
    public string Name { get; set; }
    #pragma warning restore 8618

    /// <summary>
    /// The minimum required version. Inclusive.
    /// </summary>
    [ConfigProperty(1)]
    public Version? MinVersion { get; set; }

    /// <summary>
    /// The version above the highest allowed version.
    /// </summary>
    [ConfigProperty(1)]
    public Version? MaxVersion { get; set; }

    /// <summary>
    /// Notes on the requirement.
    /// </summary>
    public string? Notes { get; set; }

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

        return sb.ToString();
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Requirement r && ToTuple.Equals(r.ToTuple);

    /// <inheritdoc />
    public override int GetHashCode() => ToTuple.GetHashCode();

    private object ToTuple => (Name, MinVersion, MaxVersion, Notes);

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

            yield return new Requirement
            {
                MaxVersion = highestMaxVersion,
                MinVersion = lowestMinVersion,
                Name       = group.Key,
                Notes      = text
            };
        }
    }
}

}
