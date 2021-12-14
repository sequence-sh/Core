using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Reductech.EDR.Core.Internal;

/// <summary>
/// Base class for all SCL Objects
/// </summary>
public interface ISCLObject
{
    /// <summary>
    /// Short name for this object
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Serialize this object
    /// </summary>
    [Pure]
    string Serialize();

    /// <summary>
    /// The Type Reference
    /// </summary>
    TypeReference TypeReference { get; }

    /// <summary>
    /// Tries to convert this SCL object to an SCL object of a different type
    /// </summary>
    [Pure]
    public Result<ISCLObject, IErrorBuilder> TryConvert(Type type, string propertyName)
    {
        if (type.IsInstanceOfType(this))
            return Result.Success<ISCLObject, IErrorBuilder>(this);

        var method  = GetType().GetMethod(nameof(TryConvertTyped))!;
        var generic = method.MakeGenericMethod(type);
        var result  = generic.Invoke(this, new object?[] { propertyName });

        return (Result<ISCLObject, IErrorBuilder>)result!;
    }

    /// <summary>
    /// Tries to convert this SCL object to an SCL object of a different type
    /// </summary>
    [Pure]
    sealed Result<T, IErrorBuilder> TryConvertTyped<T>(string propertyName) where T : ISCLObject
    {
        var r = MaybeAs<T>();

        if (r.HasNoValue)
            return ErrorCode.InvalidCast.ToErrorBuilder(propertyName, Name);

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
            prefix + Serialize() + suffix
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
        var instance = Activator.CreateInstance(
            typeof(T),
            BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public
        )!;

        var r = (T)((T)instance).DefaultValue;

        return r;
    }

    /// <summary>
    /// The default value for an SCL object of this type
    /// </summary>
    ISCLObject DefaultValue { get; } //TODO replace with a static method in dotnet 7

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
            JsonElement je                                => Entity.Create(je),
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
