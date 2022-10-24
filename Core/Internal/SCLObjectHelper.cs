namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// Contains methods for converting C Sharp objects to SCL Objects
/// </summary>
public static class SCLObjectHelper
{
    /// <summary>
    /// Convert this enum to an scl enum
    /// </summary>
    public static ISCLEnum ConvertToSCLEnum<T>(this T value) where T : Enum
    {
        return ConvertToSCLEnumUnsafe(value);
    }

    /// <summary>
    /// Convert this enum to an scl enum
    /// </summary>
    public static ISCLEnum ConvertToSCLEnumUnsafe(object value)
    {
        var type        = value.GetType();
        var genericType = typeof(SCLEnum<>).MakeGenericType(type);

        var constructor = genericType.GetConstructor(new[] { type });

        var obj = constructor?.Invoke(new[] { value });

        return (ISCLEnum)obj!;
    }

    /// <summary>
    /// Create an ISCLObject from a JsonElement
    /// </summary>
    public static ISCLObject ConvertToSCLObject(this JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Undefined: return SCLNull.Instance;
            case JsonValueKind.Object:
            {
                var keys   = ImmutableArray.CreateBuilder<string>();
                var values = ImmutableArray.CreateBuilder<ISCLObject>();

                var index = 0;

                foreach (var prop in element.EnumerateObject())
                {
                    keys[index]   = prop.Name;
                    values[index] = ConvertToSCLObject(prop.Value);
                    index++;
                }

                var entity = new Entity(keys.ToImmutable(), values.ToImmutable());
                return entity;
            }
            case JsonValueKind.Array:
            {
                var list = element.EnumerateArray().Select(ConvertToSCLObject).ToSCLArray();
                return list;
            }
            case JsonValueKind.String: return new StringStream(element.GetString()!);
            case JsonValueKind.Number:
            {
                if (element.TryGetInt32(out var i))
                    return new SCLInt(i);

                return new SCLDouble(element.GetDouble());
            }
            case JsonValueKind.True: return SCLBool.True;
            case JsonValueKind.False: return SCLBool.False;
            case JsonValueKind.Null: return SCLNull.Instance;
            default: throw new ArgumentOutOfRangeException(element.ValueKind.ToString());
        }
    }

    ///// <summary>
    ///// Convert this enum to a constant step
    ///// </summary>
    //public static IStep ConvertToConstantEnumStepUnsafe(object value)
    //{
    //    throw new NotImplementedException();
    //    //var type = value.GetType();
    //}

    /// <summary>
    /// Convert this bool to an scl bool
    /// </summary>
    public static SCLBool ConvertToSCLObject(this bool b) => b ? SCLBool.True : SCLBool.False;

    /// <summary>
    /// Convert this int to an scl int
    /// </summary>
    public static SCLInt ConvertToSCLObject(this int i) => new(i);

    /// <summary>
    /// Convert this double to an scl double
    /// </summary>
    public static SCLDouble ConvertToSCLObject(this double d) => new(d);

    /// <summary>
    /// Convert this dateTime to an scl dateTime
    /// </summary>
    public static SCLDateTime ConvertToSCLObject(this DateTime dt) => new(dt);
}
