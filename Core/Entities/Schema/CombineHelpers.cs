using System;

namespace Reductech.EDR.Core.Entities.Schema
{

/// <summary>
/// Contains methods to help with combining values
/// </summary>
public static class CombineHelpers
{
    /// <summary>
    /// Combine two structs
    /// </summary>
    public static T? Combine<T>(
        T? t1,
        T? t2,
        Func<T, T, T?> chooseFunc)
        where T : struct
    {
        if (t1 is null)
            return t2;

        if (t2 is null)
            return t1;

        if (t1.Value.Equals(t2.Value))
            return t1;

        return chooseFunc(t1.Value, t2.Value);
    }

    /// <summary>
    /// Combine two objects
    /// </summary>
    public static T? Combine<T>(
        T? t1,
        T? t2,
        Func<T, T, T?> chooseFunc)
        where T : class
    {
        if (t1 is null)
            return t2;

        if (t2 is null)
            return t1;

        if (t1 == t2)
            return t1;

        return chooseFunc(t1, t2)!;
    }
}

}
