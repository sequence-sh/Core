using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Namotion.Reflection;
using Reductech.EDR.Core.Attributes;

namespace Reductech.EDR.Core.Internal.Documentation
{

/// <summary>
/// A wrapper for this documented object.
/// </summary>
public class StepWrapper : IDocumented
{
    /// <summary>
    /// Creates a new StepWrapper.
    /// </summary>
    public StepWrapper(IGrouping<IStepFactory, string> grouping)
    {
        Factory               = grouping.Key;
        DocumentationCategory = grouping.Key.Category;

        RelevantProperties = grouping.Key.StepType.GetProperties()
            .Select(
                property => (
                    property, attribute: property.GetCustomAttribute<StepPropertyBaseAttribute>())
            )
            .Where(x => x.attribute != null)
            // ReSharper disable once ConstantConditionalAccessQualifier
            .OrderBy(x => x.attribute!.Order)
            .ThenBy(x => x.property.Name)
            .Select(x => x.property)
            .ToList();

        Parameters =
            RelevantProperties.Select(GetPropertyWrapper).ToList(); //TODO get default values

        Requirements = grouping.Key.Requirements.Select(x => $"Requires {x}").ToList();

        TypeDetails = grouping.Key.OutputTypeExplanation;

        AllNames = grouping.ToList();
    }

    private static PropertyWrapper GetPropertyWrapper(PropertyInfo propertyInfo) =>
        new(propertyInfo, null);

    private IStepFactory Factory { get; }

    /// <inheritdoc />
    public string DocumentationCategory { get; }

    /// <inheritdoc />
    public string Name => TypeNameHelper.GetHumanReadableTypeName(Factory.StepType);

    /// <inheritdoc />
    public string FileName => Factory.TypeName + ".md";

    /// <inheritdoc />
    public string Summary => Factory.StepType.GetXmlDocsSummary();

    /// <inheritdoc />
    public string? TypeDetails { get; }

    /// <inheritdoc />
    public IEnumerable<string> Requirements { get; }

    /// <summary>
    /// Properties of this step.
    /// </summary>
    protected IEnumerable<PropertyInfo> RelevantProperties { get; }

    /// <inheritdoc />
    public IEnumerable<IParameter> Parameters { get; }

    /// <inheritdoc />
    public IReadOnlyList<string> AllNames { get; }

    /// <summary>
    /// The wrapper for a property.
    /// </summary>
    protected class PropertyWrapper : IParameter
    {
        private readonly PropertyInfo _propertyInfo;

        /// <summary>
        /// Creates a new PropertyWrapper.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="defaultValueString"></param>
        public PropertyWrapper(PropertyInfo propertyInfo, string? defaultValueString)
        {
            _propertyInfo = propertyInfo;

            Required = _propertyInfo.GetCustomAttributes<RequiredAttribute>().Any()
                    && defaultValueString == null;

            var explanation = Required
                ? null
                : //Required properties should not have a default value
                propertyInfo.GetCustomAttribute<DefaultValueExplanationAttribute>()?.Explanation;

            var extraFields = new Dictionary<string, string>();

            var dvs = explanation == null
                ? defaultValueString
                : $"{explanation}"; //TODO make italic somehow

            if (!string.IsNullOrWhiteSpace(dvs))
                extraFields.Add("Default Value", dvs);

            AddFieldFromAttribute<ExampleAttribute>(
                "Example",
                extraFields,
                propertyInfo,
                x => x.Example
            );

            AddFieldFromAttribute<RequiredVersionAttribute>(
                "Requirements",
                extraFields,
                propertyInfo,
                x => x.Text
            );

            AddFieldFromAttribute<DocumentationURLAttribute>(
                "URL",
                extraFields,
                propertyInfo,
                x => $"[{propertyInfo.Name}]({x.DocumentationURL})"
            );

            AddFieldFromAttribute<RecommendedRangeAttribute>(
                "Recommended Range",
                extraFields,
                propertyInfo,
                x => x.RecommendedRange
            );

            AddFieldFromAttribute<RecommendedValueAttribute>(
                "Recommended Value",
                extraFields,
                propertyInfo,
                x => x.RecommendedValue
            );

            AddFieldFromAttribute<AllowedRangeAttribute>(
                "Allowed Range",
                extraFields,
                propertyInfo,
                x => x.AllowedRangeValue
            );

            AddFieldFromAttribute<SeeAlsoAttribute>(
                "See Also",
                extraFields,
                propertyInfo,
                x => x.SeeAlso
            );

            AddFieldFromAttribute<ValueDelimiterAttribute>(
                "Value Delimiter",
                extraFields,
                propertyInfo,
                x => x.Delimiter
            );

            ExtraFields = extraFields;

            Type = _propertyInfo.PropertyType.IsGenericType
                ? _propertyInfo.PropertyType.GetGenericArguments()[0]
                : _propertyInfo.PropertyType;
        }

        private static void AddFieldFromAttribute<T>(
            string name,
            IDictionary<string, string> dictionary,
            MemberInfo propertyInfo,
            Func<T, string> getAttributeText) where T : Attribute
        {
            var t = propertyInfo.GetCustomAttribute<T>();

            if (t == null)
                return;

            var attributeText = getAttributeText(t);

            if (!string.IsNullOrWhiteSpace(attributeText))
                dictionary.Add(name, attributeText);
        }

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name => _propertyInfo.Name;

        /// <summary>
        /// A summary of the property.
        /// </summary>
        public string Summary => _propertyInfo.GetXmlDocsSummary();

        /// <summary>
        /// The type of this property.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Whether this property must be set.
        /// </summary>
        public bool Required { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, string> ExtraFields { get; }
    }
}

}
