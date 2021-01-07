using Reductech.EDR.Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
    /// The The version above the highest allowed version.
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
    public override bool Equals(object? obj)
    {
        return obj is Requirement r && ToTuple.Equals(r.ToTuple);
    }

    /// <inheritdoc />
    public override int GetHashCode() => ToTuple.GetHashCode();

    private object ToTuple => (Name, MinVersion, MaxVersion, Notes);
}

}
