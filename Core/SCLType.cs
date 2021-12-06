using System;

namespace Reductech.EDR.Core
{

/// <summary>
/// A basic type in the SCL Type system
/// </summary>
public enum SCLType
{
    /// <summary>
    /// A string.
    /// </summary>
    String,

    /// <summary>
    /// An integer.
    /// </summary>
    Integer,

    /// <summary>
    /// A double precision number.
    /// </summary>
    Double,

    /// <summary>
    /// An enumeration of some sort.
    /// The format string will contain the possible values.
    /// </summary>
    Enum,

    /// <summary>
    /// A boolean.
    /// </summary>
    Bool,

    /// <summary>
    /// A date.
    /// </summary>
    Date,

    /// <summary>
    /// Another entity
    /// </summary>
    Entity,

    /// <summary>
    /// The null value
    /// </summary>
    Null
}

/// <summary>
/// Contains methods to help with SCL types
/// </summary>
public static class SCLTypeHelper
{
    /// <summary>
    /// Gets the C# type this SCL type represents
    /// </summary>
    public static Type GetCSharpType(this SCLType sclType)
    {
        return sclType switch
        {
            SCLType.String  => typeof(StringStream),
            SCLType.Integer => typeof(int),
            SCLType.Double  => typeof(double),
            SCLType.Enum    => typeof(Enum),
            SCLType.Bool    => typeof(bool),
            SCLType.Date    => typeof(DateTime),
            SCLType.Entity  => typeof(Entity),
            SCLType.Null    => typeof(SCLNull),
            _               => throw new ArgumentOutOfRangeException(nameof(sclType), sclType, null)
        };
    }

    /// <summary>
    /// Gets the SCL type from the c sharp type
    /// </summary>
    public static SCLType? GetSCLType(this Type cSharpType)
    {
        if (cSharpType.IsEnum)
            return SCLType.Enum;

        if (cSharpType == typeof(string) || cSharpType == typeof(StringStream))
            return SCLType.String;

        if (cSharpType == typeof(int))
            return SCLType.Integer;

        if (cSharpType == typeof(double))
            return SCLType.Double;

        if (cSharpType == typeof(bool))
            return SCLType.Bool;

        if (cSharpType == typeof(DateTime))
            return SCLType.Date;

        if (cSharpType == typeof(Entity))
            return SCLType.Entity;

        if (cSharpType == typeof(SCLNull))
            return SCLType.Null;

        return null;
    }
}

}
