using Generator.Equals;
using Namotion.Reflection;

namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A step parameter that uses a property info
/// </summary>
[Equatable]
public sealed partial record StepParameter : IStepParameter
{
    /// <summary>
    /// A step parameter that uses a property info
    /// </summary>
    private StepParameter(
        PropertyInfo propertyInfo,
        StepPropertyBaseAttribute attribute)
    {
        PropertyInfo = propertyInfo;
        Attribute = attribute;
        Required = PropertyInfo.GetCustomAttributes<RequiredAttribute>().Any();
        Aliases = PropertyInfo.GetCustomAttributes<AliasAttribute>().Select(x => x.Name).ToList();
        Summary = propertyInfo.GetXmlDocsSummary();

        Metadata = PropertyInfo.GetCustomAttributes<MetadataAttribute>()
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        StepType = PropertyInfo.PropertyType;

        static Type TryGetNested(Type t) => t.IsGenericType ? t.GetGenericArguments()[0] : t;

        if (Attribute is StepPropertyAttribute)
        {
            ActualType = TryGetNested(PropertyInfo.PropertyType);
        }
        else if (Attribute is StepListPropertyAttribute)
        {
            ActualType = Type.MakeGenericSignatureType(
                typeof(List<>),
                TryGetNested(TryGetNested(PropertyInfo.PropertyType))
            );
        }
        else if (Attribute is VariableNameAttribute)
        {
            ActualType = typeof(VariableName);
        }
        else
            ActualType = TryGetNested(PropertyInfo.PropertyType);

        MemberType = Attribute.MemberType;

        var extraFields = new Dictionary<string, string>();

        var explanation = Required
            ? null
            : //Required properties should not have a default value
            propertyInfo.GetCustomAttributes<DefaultValueExplanationAttribute>()
                .FirstOrDefault()
                ?.Explanation;

        var dvs = $"{explanation}";

        if (!string.IsNullOrWhiteSpace(dvs))
            extraFields.Add("Default Value", dvs);

        foreach (var stepPropertyMetadataAttribute in propertyInfo
                     .GetCustomAttributes<StepPropertyMetadataAttribute>()
                     .GroupBy(x => x.MetadataFieldName))
        {
            var value = stepPropertyMetadataAttribute.Select(x => x.MetadataFieldValue).First();
            extraFields[stepPropertyMetadataAttribute.Key] = value;
        }

        ExtraFields = extraFields;
    }

    /// <summary>
    /// Create this Step Parameter
    /// </summary>
    public static IStepParameter? TryCreate(PropertyInfo propertyInfo)
    {
        var attribute = propertyInfo.GetCustomAttribute<StepPropertyBaseAttribute>();

        if (attribute is null)
            return null;

        return new StepParameter(
            propertyInfo,
            attribute
        );
    }

    /// <summary>
    /// The property Info
    /// </summary>
    [IgnoreEquality]
    public PropertyInfo PropertyInfo { get; }

    /// <summary>
    /// The Attribute
    /// </summary>
    [IgnoreEquality]
    public StepPropertyBaseAttribute Attribute { get; }

    /// <inheritdoc />
    public bool Required { get; }

    /// <inheritdoc />
    public string Name => PropertyInfo.Name;

    /// <inheritdoc />
    public int? Order => Attribute.Order;

    /// <inheritdoc />
    [IgnoreEquality]
    public Type StepType { get; }

    /// <inheritdoc />
    [IgnoreEquality]
    public Type ActualType { get; }

    /// <inheritdoc />
    [IgnoreEquality]
    public IReadOnlyCollection<string> Aliases { get; }

    /// <inheritdoc />
    [IgnoreEquality]
    public string Summary { get; }

    /// <inheritdoc />
    [IgnoreEquality]
    public IReadOnlyDictionary<string, string> ExtraFields { get; }

    /// <inheritdoc />
    [IgnoreEquality]
    public IReadOnlyDictionary<string, string> Metadata { get; }

    /// <inheritdoc />
    public MemberType MemberType { get; }

    /// <inheritdoc />
    public override string ToString() => Name;
}
