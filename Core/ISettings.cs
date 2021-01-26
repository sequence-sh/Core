using System.Collections.Generic;

namespace Reductech.EDR.Core
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// External settings for running the step.
/// </summary>
public record SCLSettings(SCLSettingsValue.Map Map)
{
    public const string VersionKey = "Version";

    public static readonly SCLSettings EmptySettings =
        new(new SCLSettingsValue.Map(new Dictionary<string, SCLSettingsValue>()));
}

public abstract record SCLSettingsValue
{
    public record Primitive(string Value) : SCLSettingsValue;

    public record Map(IReadOnlyDictionary<string, SCLSettingsValue> Dictionary) : SCLSettingsValue;
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

}
