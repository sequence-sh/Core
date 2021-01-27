using System;

namespace Reductech.EDR.Core.Attributes
{

/// <summary>
/// Use this attribute to denote the required version of some software.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RequiredVersionAttribute : Attribute
{
    /// <summary>
    /// Create a new RequiredVersion attribute
    /// </summary>
    /// <param name="softwareName">e.g. "Nuix"</param>
    /// <param name="minRequiredVersion">e.g. "6.2"</param>
    /// <param name="maxRequiredVersion">e.g. "8.4"</param>
    /// <param name="notes">Special notes</param>
    /// <param name="features">Features</param>
    public RequiredVersionAttribute(
        string softwareName,
        string? minRequiredVersion,
        string? maxRequiredVersion = null,
        string? notes = null,
        params string[] features)
    {
        SoftwareName       = softwareName;
        Notes              = notes;
        Features           = features;
        MinRequiredVersion = minRequiredVersion == null ? null : new Version(minRequiredVersion);
        MaxRequiredVersion = maxRequiredVersion == null ? null : new Version(maxRequiredVersion);
    }

    /// <summary>
    /// The software whose version is required.
    /// </summary>
    public string SoftwareName { get; }

    /// <summary>
    /// The minimum required version. Inclusive.
    /// </summary>
    public Version? MinRequiredVersion { get; }

    /// <summary>
    /// The version above the highest allowed version.
    /// </summary>
    public Version? MaxRequiredVersion { get; }

    /// <summary>
    /// The notes.
    /// </summary>
    public string? Notes { get; }

    /// <summary>
    /// The required features
    /// </summary>
    public string[] Features { get; }

    /// <summary>
    /// The required version in human readable form.
    /// </summary>
    public string Text
    {
        get
        {
            if (MinRequiredVersion != null && MaxRequiredVersion != null)
                return $"{SoftwareName} {MinRequiredVersion}-{MaxRequiredVersion}";

            if (MinRequiredVersion != null)
                return $"{SoftwareName} {MinRequiredVersion}";

            if (MaxRequiredVersion != null)
                return $"{SoftwareName} Up To {MaxRequiredVersion}";

            return SoftwareName;
        }
    }

    /// <inheritdoc />
    public override string ToString() => Text;

    /// <summary>
    /// Convert this to a requirement.
    /// </summary>
    public Requirement ToRequirement()
    {
        return new()
        {
            MaxVersion = MaxRequiredVersion,
            MinVersion = MinRequiredVersion,
            Name       = SoftwareName,
            Notes      = Notes,
            Features   = Features
        };
    }
}

}
