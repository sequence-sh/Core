using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Common;
using Namotion.Reflection;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Type = System.Type;

namespace Reductech.EDR.Core.TestHarness
{
    public interface IStepTestBase
    {
        string StepName { get; }

        Type StepType { get; }
    }



    public abstract partial class StepTestBase<TStep, TOutput> : IStepTestBase
        where TStep : class, ICompoundStep<TOutput>, new()
    {
        protected StepTestBase(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        public string StepName => typeof(TStep).GetDisplayName();

        /// <inheritdoc />
        public Type StepType => typeof(TStep);


        public ITestOutputHelper TestOutputHelper { get; }


        [Fact]
        public void All_properties_should_be_required_or_have_default_values_and_attributes()
        {
            var instance = new TStep();
            var errors = new List<string>();

            foreach (var propertyInfo in typeof(TStep).GetProperties()
                .Where(x => x.IsDecoratedWith<StepPropertyBaseAttribute>()))
            {
                var propName = $"{typeof(TStep).GetDisplayName()}.{propertyInfo.Name}";


                var required = propertyInfo.IsDecoratedWith<RequiredAttribute>();
                var hasDefaultAttribute = propertyInfo.IsDecoratedWith<DefaultValueExplanationAttribute>();

                if (propertyInfo.SetMethod == null || !propertyInfo.SetMethod.IsPublic)
                    errors.Add($"{propName} has no public setter");

                if (propertyInfo.GetMethod == null || !propertyInfo.GetMethod.IsPublic)
                {
                    errors.Add($"{propName} has no public getter");
                    continue;
                }

                var defaultValue = propertyInfo.GetValue(instance);

                if (required)
                {
                    if (hasDefaultAttribute)
                        errors.Add($"{propName} has both required and defaultValueExplanation attributes");

                    if (defaultValue != null && !(defaultValue is VariableName vn && vn == default))
                        errors.Add($"{propName} is required but it has a default value");
                }
                else if (hasDefaultAttribute)
                {
                    if (propertyInfo.CustomAttributes.All(x => x.AttributeType.Name != "NullableAttribute") &&
                        defaultValue == null)
                        errors.Add(
                            $"{propName} has a default value explanation but is not nullable and it's default value is null");
                }
                else
                {
                    errors.Add($"{propName} has neither required nor defaultValueExplanation attributes");
                }
            }

            if (errors.Any())
                throw new XunitException(string.Join("\r\n", errors));
        }


        [Fact]
        public void Process_factory_must_be_set_correctly()
        {
            var instance = new TStep();

            instance.StepFactory.Should().NotBeNull();

            instance.StepFactory.StepType.Name.Should()
                .Be(typeof(TStep).Name); //Compare type names because of generic types

            var stepFactoryType = instance.StepFactory.GetType();

            var constructor = stepFactoryType.GetConstructor(System.Array.Empty<Type>());
            constructor.Should().BeNull($"{StepName} should not have a public parameterless constructor");

            var instanceProperty = stepFactoryType.GetProperty("Instance",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);

            instanceProperty.Should()
                .NotBeNull($"{StepName} should have a public static Get property named 'Instance'");
            instanceProperty.Should().NotBeWritable($"{StepName}.Instance should be readonly");

            instanceProperty!.PropertyType.Should()
                .BeAssignableTo<IStepFactory>($"{StepName}.Instance should return an IStepFactory");
        }


        public static Constant<TNew> Constant<TNew>(TNew value) => new Constant<TNew>(value);

        public static IStep<List<TNew>> Array<TNew>(params TNew[] elements)=> new Array<TNew>() {Elements = elements.Select(Constant).ToList()};

        public static IStep<TNew> GetVariable<TNew>(string variableName)=> new GetVariable<TNew>() {VariableName = new VariableName(variableName)};
        public static IStep<TNew> GetVariable<TNew>(VariableName variableName)=> new GetVariable<TNew>() {VariableName = variableName};

        protected static Entity CreateEntity(params (string key, string value)[] pairs)
        {
            var evs = pairs
                .GroupBy(x=>x.key, x=>x.value)
                .Select(x => new KeyValuePair<string, EntityValue>(x.Key, EntityValue.Create(x)));

            return new Entity(evs);
        }

        protected static Schema CreateSchema(string name, bool allowExtraProperties, params (string propertyName, SchemaPropertyType type, Multiplicity multiplicity)[] properties)
        {
            return new Schema()
            {
                Name = name,
                AllowExtraProperties = allowExtraProperties,
                Properties = properties.ToDictionary(x=>x.propertyName, x=> new SchemaProperty()
                {
                    Multiplicity = x.multiplicity,
                    Type = x.type
                })
            };
        }

        protected static Schema CreateSchema(string name, bool allowExtraProperties, params (string propertyName, SchemaPropertyType type, Multiplicity multiplicity, string? regex, List<string>? format)[] properties)
        {
            return new Schema()
            {
                Name = name,
                AllowExtraProperties = allowExtraProperties,
                Properties = properties.ToDictionary(x => x.propertyName, x => new SchemaProperty()
                {
                    Multiplicity = x.multiplicity,
                    Type = x.type,
                    Regex = x.regex,
                    Format = x.format
                })
            };
        }

        protected static string CompressNewlines(string s) => s.Replace("\r\n", "\n");
    }
}