using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy.Internal;
using FluentAssertions;
using Namotion.Reflection;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Reductech.EDR.Core.TestHarness
{
    public abstract partial class StepTestBase<TStep, TOutput> where TStep : class, ICompoundStep<TOutput>, new()
    {
        protected StepTestBase(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        public string StepName => typeof(TStep).GetDisplayName();


        public ITestOutputHelper TestOutputHelper {get;}


        [Fact]
        public void All_properties_should_be_required_or_have_default_values_and_attributes()
        {
            var instance = CreateInstance();
            var errors = new List<string>();

            foreach (var propertyInfo in typeof(TStep).GetProperties().Where(x=>x.GetCustomAttribute<StepPropertyBaseAttribute>() != null))
            {
                var propName = $"{typeof(TStep).GetDisplayName()}.{propertyInfo.Name}";


                var required = propertyInfo.GetCustomAttribute<RequiredAttribute>() != null;
                var hasDefaultAttribute = propertyInfo.GetCustomAttribute<DefaultValueExplanationAttribute>() != null;

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
                    if(hasDefaultAttribute)
                        errors.Add($"{propName} has both required and defaultValueExplanation attributes");

                    if(defaultValue != null)
                        errors.Add($"{propName} is required but it has a default value");
                }
                else if(hasDefaultAttribute)
                {
                    if(!propertyInfo.PropertyType.IsNullableType() && defaultValue == null)
                        errors.Add($"{propName} has a default value explanation but is not nullable and it's default value is null");
                }
                else
                {
                    errors.Add($"{propName} has neither required nor defaultValueExplanation attributes");
                }
            }

            if(errors.Any())
                throw new XunitException(string.Join("\r\n", errors));

        }


        [Fact]
        public void Process_factory_must_be_set_correctly()
        {
            var instance = CreateInstance();

            instance.StepFactory.Should().NotBeNull();

            instance.StepFactory.StepType.Should().Be(typeof(TStep));

            var stepFactoryType = instance.StepFactory.GetType();

            var constructor = stepFactoryType.GetConstructor(new Type[] { });
            constructor.Should().BeNull($"{StepName} should not have a public parameterless constructor");

            var instanceProperty = stepFactoryType.GetProperty("Instance",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);

            instanceProperty.Should().NotBeNull($"{StepName} should have a public static Get property named 'Instance'");
            instanceProperty.Should().NotBeWritable($"{StepName}.Instance should be readonly");

            instanceProperty!.PropertyType.Should().BeAssignableTo<IStepFactory>($"{StepName}.Instance should return an IStepFactory");
        }

        private static TStep CreateInstance() => Activator.CreateInstance<TStep>();

        public static IStep<TNew> Constant<TNew>(TNew value) => new Constant<TNew>(value);
    }
}