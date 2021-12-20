using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// Base class for all SCL Objects
/// </summary>
public interface ISCLObject
{
    /// <summary>
    /// Serialize this object
    /// </summary>
    [Pure]
    string Serialize(SerializeOptions options);

    /// <summary>
    /// The Type Reference
    /// </summary>
    [Pure]
    TypeReference GetTypeReference();

    /// <summary>
    /// Tries to convert this SCL object to an SCL object of a different type
    /// </summary>
    [Pure]
    public Result<ISCLObject, IErrorBuilder> TryConvert(Type type, string propertyName)
    {
        if (type.IsInstanceOfType(this))
            return Result.Success<ISCLObject, IErrorBuilder>(this);

        var method = typeof(ISCLObject).GetMethod(
            nameof(TryConvertTyped1),
            BindingFlags.NonPublic | BindingFlags.Instance
        )!;

        var generic = method.MakeGenericMethod(type);
        var result  = generic.Invoke(this, new object?[] { propertyName });

        return (Result<ISCLObject, IErrorBuilder>)result!;
    }

    private Result<ISCLObject, IErrorBuilder> TryConvertTyped1<T>(string propertyName)
        where T : ISCLObject
    {
        return TryConvertTyped<T>(propertyName).Map(x => x as ISCLObject);
    }

    /// <summary>
    /// Tries to convert this SCL object to an SCL object of a different type
    /// </summary>
    [Pure]
    Result<T, IErrorBuilder> TryConvertTyped<T>(string propertyName) where T : ISCLObject
    {
        var r = MaybeAs<T>();

        if (r.HasNoValue)
        {
            //Special case for OneOf
            if (typeof(T).IsGenericType && typeof(T).GetInterfaces().Contains(typeof(ISCLOneOf)))
            {
                var createMethod =
                    typeof(T).GetMethod(
                        nameof(SCLOneOf<ISCLObject, ISCLObject>.TryCreate),
                        BindingFlags.Public | BindingFlags.Static
                    )!;

                var createResult = (Maybe<T>)createMethod.Invoke(
                    null,
                    new object?[] { propertyName, this }
                )!;

                if (createResult.HasValue)
                {
                    return createResult.Value;
                }
            }

            return ErrorCode.InvalidCast.ToErrorBuilder(
                propertyName,
                Serialize(SerializeOptions.Name)
            );
        }

        return r.Value;
    }

    /// <summary>
    /// Returns this as a T if possible
    /// </summary>
    [Pure]
    Maybe<T> MaybeAs<T>() where T : ISCLObject;

    /// <summary>
    /// Convert this SCL object to a CSharp object
    /// </summary>
    /// <returns></returns>
    [Pure]
    object? ToCSharpObject();

    /// <summary>
    /// Format this object as a multiline indented string
    /// </summary>
    public virtual void Format(
        StringBuilder stringBuilder,
        int indentation,
        FormattingOptions options,
        string? prefix = null,
        string? suffix = null)
    {
        AppendLineIndented(
            stringBuilder,
            indentation,
            prefix + Serialize(SerializeOptions.Serialize) + suffix
        );
    }

    /// <summary>
    /// Append a line to a StringBuilder
    /// </summary>
    static void AppendLineIndented(StringBuilder sb, int indentation, string value)
    {
        sb.Append('\t', indentation);
        sb.AppendLine(value);
    }

    /// <summary>
    /// Gets the default value for a particular SCL Type
    /// </summary>
    [Pure]
    public static T GetDefaultValue<T>() where T : ISCLObject
    {
        if (Unit.Default is T unit)
            return unit;

        if (SCLNull.Instance is T sclNull)
            return sclNull;

        if (Entity.Empty is T entity)
            return entity;

        if (StringStream.Empty is T stringStream)
            return stringStream;

        if (SCLBool.False is T sclBool)
            return sclBool;

        if (new SCLInt(0) is T sclInt)
            return sclInt;

        if (new SCLDouble(0) is T sclDouble)
            return sclDouble;

        if (new SCLDateTime(DateTime.MinValue) is T sclDateTime)
            return sclDateTime;

        if (typeof(T).IsGenericType)
        {
            if (typeof(T).GetInterfaces().Contains(typeof(IArray)))
            {
                var eagerArrayType =
                    typeof(EagerArray<>).MakeGenericType(typeof(T).GenericTypeArguments[0]);

                var array = Activator.CreateInstance(eagerArrayType);
                return (T)array;
            }

            if (typeof(T).GetInterfaces().Contains(typeof(ISCLEnum)))
            {
                var enumType  = typeof(T).GenericTypeArguments[0];
                var enumValue = Enum.GetValues(enumType).OfType<object>().First();
                var result    = Activator.CreateInstance(typeof(T), enumValue);
                return (T)result;
            }

            if (typeof(T).GetInterfaces().Contains(typeof(ISCLOneOf)))
            {
                var method =
                    typeof(T).GetMethod(
                        nameof(SCLOneOf<SCLNull, SCLNull>.GetDefaultValue),
                        BindingFlags.Public | BindingFlags.Static
                    );

                var result = method.Invoke(null, null);

                return (T)result;
            }
        }

        throw new Exception($"Cannot Get Default Value of type {typeof(T).Name}");
    }

    //NOTE: this must be a method to prevent recursion
    //TODO replace with a static method in dotnet 7

    /// <summary>
    /// Gets a schema node which could match this SCL Object
    /// </summary>
    [Pure]
    SchemaNode ToSchemaNode(
        string path,
        SchemaConversionOptions? schemaConversionOptions);

    /// <summary>
    /// Convert this object to a Json Element
    /// </summary>
    /// <returns></returns>
    [Pure]
    public virtual JsonElement ToJsonElement()
    {
        var element = ToCSharpObject()
            !.ToJsonDocument(DefaultJsonSerializerOptions)
            .RootElement.Clone();

        return element;
    }

    /// <summary>
    /// Json Serializer options to use
    /// </summary>
    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Create an SCL object from a JSON element
    /// </summary>
    public static ISCLObject CreateFromJElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Undefined: return Entity.Create(element);
            case JsonValueKind.Object:    return Entity.Create(element);
            case JsonValueKind.Array:
            {
                var enumerator = element.EnumerateArray();
                var list       = new List<ISCLObject>(element.GetArrayLength());

                while (enumerator.MoveNext())
                {
                    var arrayElement = CreateFromJElement(enumerator.Current);
                    list.Add(arrayElement);
                }

                return list.ToSCLArray();
            }
            case JsonValueKind.String: return new StringStream(element.GetString()!);
            case JsonValueKind.Number:
            {
                if (element.TryGetInt32(out var i))
                    return new SCLInt(i);

                return new SCLDouble(element.GetDouble());
            }
            case JsonValueKind.True:  return SCLBool.True;
            case JsonValueKind.False: return SCLBool.False;
            case JsonValueKind.Null:  return SCLNull.Instance;
            default:                  throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Create an SCL object from a CSharp object
    /// </summary>
    [Pure]
    public static ISCLObject CreateFromCSharpObject(object? o)
    {
        return o switch
        {
            ISCLObject obj                                => obj,
            string s                                      => new StringStream(s),
            int i                                         => new SCLInt(i),
            long ln and < int.MaxValue and > int.MinValue => new SCLInt(Convert.ToInt32(ln)),
            byte @byte                                    => new SCLInt(@byte),
            short @short                                  => new SCLInt(@short),
            double d                                      => new SCLDouble(d),
            bool b                                        => SCLBool.Create(b),
            DateTime dateTime                             => new SCLDateTime(dateTime),
            Enum e                                        => new StringStream(e.GetDisplayName()),
            Version version                               => new StringStream(version.ToString()),
            JsonElement je                                => CreateFromJElement(je),
            IEntityConvertible entityConvertible          => entityConvertible.ConvertToEntity(),
            IOneOf oneOf                                  => CreateFromCSharpObject(oneOf.Value),
            IDictionary dict                              => Entity.Create(dict),
            IEnumerable enumerable => enumerable
                .OfType<object>()
                .Select(CreateFromCSharpObject)
                .ToSCLArray(),

            null => SCLNull.Instance,
            _    => DefaultConvert(o)
        };

        static ISCLObject DefaultConvert(object argValue)
        {
            if (argValue.GetType()
                    .GetCustomAttributes(true)
                    .OfType<SerializableAttribute>()
                    .Any() ||
                argValue.GetType()
                    .GetCustomAttributes(true)
                    .OfType<DataContractAttribute>()
                    .Any()
               )
            {
                var entity = EntityConversionHelpers.ConvertToEntity(argValue);
                return entity;
            }

            throw new ArgumentException(
                $"Attempt to create ISCLObject from {argValue.GetType().Name}"
            );
        }
    }
}
