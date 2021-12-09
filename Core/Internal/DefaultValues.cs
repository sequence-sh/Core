using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal;

/// <summary>
/// Contains helper methods for getting default values
/// </summary>
public static class DefaultValues
{
    /// <summary>
    /// Gets the default value for a type
    /// </summary>
    public static T GetDefault<T>()
    {
        if (Unit.Default is T tUnit)
            return tUnit;

        if (StringStream.Empty is T tStringStream)
            return tStringStream;

        return default(T); //TODO do better
    }
}
