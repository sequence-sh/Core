using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Namotion.Reflection;
using Reductech.EDR.Utilities.Processes.attributes;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.Utilities.InstantConsole;
using YamlDotNet.Serialization;
using Process = Reductech.EDR.Utilities.Processes.mutable.Process;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// A wrapper for a runnable process.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ProcessWrapper<T> : YamlObjectWrapper, IRunnable where T : IProcessSettings
    {
        private readonly Type _processType;
        private readonly T _processSettings;

        /// <summary>
        /// Creates a new ProcessWrapper.
        /// </summary>
        public ProcessWrapper(Type processType, T processSettings, DocumentationCategory category)
        : base(processType, category)
        {
            _processType = processType;
            _processSettings = processSettings;
        }

        /// <summary>
        /// Gets an invocation of this process.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public Result<Func<object?>, List<string?[]>> TryGetInvocation(IReadOnlyDictionary<string, string> dictionary)
        {
            var errors = new List<string?[]>();
            var usedArguments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!(Activator.CreateInstance(_processType) is Process instance))
                return Result.Failure<Func<object?>, List<string?[]>>(new List<string?[]>{new []{"Instance must not be null"}});

            foreach (var property in RelevantProperties)
            {
                if (dictionary.TryGetValue(property.Name, out var v))
                {
                    usedArguments.Add(property.Name);
                    var (parsed, _, vObject ) = ArgumentHelpers.TryParseArgument(v, property.PropertyType);
                    if (parsed)
                        property.SetValue(instance, vObject);
                    else
                        errors.Add(new []{property.Name, property.PropertyType.Name, $"Could not parse '{v}'" });
                }
                else if (property.CustomAttributes.Any(att=>att.AttributeType == typeof(RequiredAttribute)))
                    errors.Add(new []{property.Name, property.PropertyType.Name, "Is required"});
            }

            var extraArguments = dictionary.Keys.Where(k => !usedArguments.Contains(k)).ToList();
            errors.AddRange(extraArguments.Select(extraArgument => new[] {extraArgument, null, "Not a valid argument"}));

            if (errors.Any())
                return Result.Failure<Func<object?>, List<string?[]>>(errors);


            var (isSuccess, _, value, error) = instance.TryFreeze<Unit>(_processSettings);

            if (isSuccess)
            {
                var func = new Func<object?>(() => value.Execute());

                return Result.Success<Func<object?>, List<string?[]>>(func);
            }

            return Result.Failure<Func<object?>, List<string?[]>>(new List<string?[]> {new []{error}});
        }
    }


    /// <summary>
    /// A wrapper for this documented object.
    /// </summary>
    public class YamlObjectWrapper : IDocumented
    {
        private readonly Type _processType;

        /// <summary>
        /// Creates a new YamlObjectWrapper.
        /// </summary>
        public YamlObjectWrapper(Type processType, DocumentationCategory category)
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

            Requirements = instance is Process process ? process.GetRequirements() : Enumerable.Empty<string>();

            TypeDetails = instance is Process process1 ? process1.GetReturnTypeInfo() : null;
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
