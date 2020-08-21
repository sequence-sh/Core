using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Namotion.Reflection;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.Utilities.InstantConsole;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// A wrapper for this documented object.
    /// </summary>
    public class YamlObjectWrapper : IDocumented
    {
        private readonly Type _processType;

        /// <summary>
        /// Creates a new YamlObjectWrapper.
        /// </summary>
        public YamlObjectWrapper(Type processType, DocumentationCategory category) //TODO switch to process factory
        {
            DocumentationCategory = category;
            _processType = processType;

            RelevantProperties = processType.GetProperties()
                .Select(p=> (p, p.GetCustomAttribute<YamlMemberAttribute>()))
                .Where(x=>x.Item2 != null)
                // ReSharper disable once ConstantConditionalAccessQualifier
                .OrderBy(x=>x.Item2?.Order)
                .ThenBy(x=>x.p.Name).Select(x=>x.p).ToList();


            var instance = Activator.CreateInstance(processType);
            Parameters = RelevantProperties.Select(propertyInfo =>
                new PropertyWrapper(propertyInfo, propertyInfo.GetValue(instance)?.ToString()  )).ToList();


            if (instance is ICompoundRunnableProcess compoundRunnableProcess)
            {
                Requirements = compoundRunnableProcess.RunnableProcessFactory.Requirements.Select(x=>x.ToString()).ToList();// reqObjects.Select(x => x.ToString()!).Distinct();

                TypeDetails = compoundRunnableProcess.RunnableProcessFactory.TypeName;// instance is IRunnableProcess process1 ? process1.GetReturnTypeInfo() : null;
            }
            else if (instance is IConstantRunnableProcess constantRunnableProcess)
            {
                Requirements = new List<string>();

                TypeDetails = constantRunnableProcess.OutputType.Name;
            }
            else
                throw new ArgumentOutOfRangeException();

            //var reqObjects = instance is IRunnableProcess process ? process.GetAllRequirements() : Enumerable.Empty<Requirement>();

            //Requirements = reqObjects.Select(x => x.ToString()!).Distinct();

            //TypeDetails = instance is IRunnableProcess process1 ? process1 .GetReturnTypeInfo() : null;
        }

        /// <inheritdoc />
        public DocumentationCategory DocumentationCategory { get; }

        /// <inheritdoc />
        public string Name => _processType.Name;

        /// <inheritdoc />
        public string Summary => _processType.GetXmlDocsSummary();

        /// <inheritdoc />
        public string? TypeDetails { get; }

        /// <inheritdoc />
        public IEnumerable<string> Requirements { get; }

        /// <summary>
        /// Properties of this process.
        /// </summary>
        protected IEnumerable<PropertyInfo> RelevantProperties { get; }

        /// <inheritdoc />
        public IEnumerable<IParameter> Parameters { get; }

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
                Required = _propertyInfo.GetCustomAttributes<RequiredAttribute>().Any() && defaultValueString == null;

                var explanation = propertyInfo.GetCustomAttribute<DefaultValueExplanationAttribute>()?.Explanation;

                var extraFields = new Dictionary<string, string>();


                var dvs = explanation == null ? defaultValueString : $"{explanation}"; //TODO make italic somehow

                if(!string.IsNullOrWhiteSpace(dvs))
                    extraFields.Add("Default Value", dvs);

                AddFieldFromAttribute<ExampleAttribute>("Example", extraFields, propertyInfo, x =>
                {
                    var equalsIndex = x.Example.IndexOf('=');

                    if (equalsIndex >= 0 && equalsIndex != x.Example.Length)
                    {
                        var angleIndex = x.Example.IndexOf('<');
                        if (angleIndex < 0 || angleIndex >= equalsIndex)
                            return x.Example.Substring(equalsIndex + 1);
                    }

                    return x.Example;
                });
                AddFieldFromAttribute<RequiredVersionAttribute>("Requirements", extraFields, propertyInfo, x=>x.Text);
                AddFieldFromAttribute<DocumentationURLAttribute>("URL", extraFields, propertyInfo, x=>$"[{propertyInfo.Name}]({ x.DocumentationURL})");
                AddFieldFromAttribute<RecommendedRangeAttribute>("Recommended Range", extraFields, propertyInfo, x=>x.RecommendedRange);
                AddFieldFromAttribute<RecommendedValueAttribute>("Recommended Value", extraFields, propertyInfo, x=>x.RecommendedValue);
                AddFieldFromAttribute<AllowedRangeAttribute>("Allowed Range", extraFields, propertyInfo, x=>x.AllowedRangeValue);
                AddFieldFromAttribute<SeeAlsoAttribute>("See Also", extraFields, propertyInfo, x=>x.SeeAlso);
                AddFieldFromAttribute<ValueDelimiterAttribute>("Value Delimiter", extraFields, propertyInfo, x=>x.Delimiter);



                ExtraFields = extraFields;
            }

            private static void AddFieldFromAttribute<T>(
                string name,
                IDictionary<string, string> dictionary, MemberInfo propertyInfo, Func<T, string> getAttributeText) where T : Attribute
            {
                var s = string.Join("\r\n", propertyInfo.GetCustomAttributes<T>().Select(getAttributeText));

                if (!string.IsNullOrWhiteSpace(s))
                {
                    dictionary.Add(name, s);
                }
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
            public Type Type => _propertyInfo.PropertyType;

            /// <summary>
            /// Whether this property must be set.
            /// </summary>
            public bool Required { get; }

            /// <inheritdoc />
            public IReadOnlyDictionary<string, string> ExtraFields { get; }
        }
    }
}
