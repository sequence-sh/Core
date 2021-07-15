using System;
using System.Collections.Immutable;

namespace Reductech.EDR.Core.Attributes
{

/// <summary>
/// Additional requirements of this property
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public abstract class RequirementAttribute : Attribute
{
    /// <summary>
    /// Convert this requirement attribute to a requirement
    /// </summary>
    public abstract Requirement ToRequirement(string connectorName);
}

/// <summary>
/// Use this attribute to denote required features.
/// </summary>
public sealed class RequiredFeatureAttribute : RequirementAttribute
{
    /// <inheritdoc />
    public RequiredFeatureAttribute(string featureKey, params string[] requiredFeatures)
    {
        FeatureKey       = featureKey;
        RequiredFeatures = requiredFeatures;
    }

    /// <summary>
    /// The key of this feature
    /// </summary>
    public string FeatureKey { get; set; }

    /// <summary>
    ///The required features
    /// </summary>
    public string[] RequiredFeatures { get; set; }

    /// <inheritdoc />
    public override Requirement ToRequirement(string connectorName)
    {
        return new FeatureRequirement(
            connectorName,
            FeatureKey,
            RequiredFeatures.ToImmutableList()
        );
    }
}

/// <summary>
/// Use this attribute to denote the required version of some software.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RequiredVersionAttribute : RequirementAttribute
{
    /// <summary>
    /// Create a new RequiredVersion attribute
    /// </summary>
    /// <param name="versionKey">e.g. "NuixVersion"</param>
    /// <param name="minRequiredVersion">e.g. "6.2"</param>
    /// <param name="maxRequiredVersion">e.g. "8.4"</param>
    /// <param name="notes">Special notes</param>
    public RequiredVersionAttribute(
        string versionKey,
        string? minRequiredVersion,
        string? maxRequiredVersion = null,
        string? notes = null)
    {
        VersionKey         = versionKey;
        Notes              = notes;
        MinRequiredVersion = minRequiredVersion == null ? null : new Version(minRequiredVersion);
        MaxRequiredVersion = maxRequiredVersion == null ? null : new Version(maxRequiredVersion);
    }

    /// <summary>
    /// The key of the version property
    /// </summary>
    public string VersionKey { get; }

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
    /// The required version in human readable form.
    /// </summary>
    public string Text
    {
        get
        {
            if (MinRequiredVersion != null && MaxRequiredVersion != null)
                return $"{VersionKey} {MinRequiredVersion}-{MaxRequiredVersion}";

            if (MinRequiredVersion != null)
                return $"{VersionKey} {MinRequiredVersion}";

            if (MaxRequiredVersion != null)
                return $"{VersionKey} Up To {MaxRequiredVersion}";

            return VersionKey;
        }
    }

    /// <inheritdoc />
    public override string ToString() => Text;

    /// <summary>
    /// Convert this to a requirement.
    /// </summary>
    public override Requirement ToRequirement(string connectorName)
    {
        return new VersionRequirement(
            connectorName,
            VersionKey,
            MinRequiredVersion,
            MaxRequiredVersion,
            Notes
        );
    }
}

}
