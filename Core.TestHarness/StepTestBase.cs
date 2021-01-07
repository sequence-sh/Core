using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Common;
using Namotion.Reflection;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

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
    protected StepTestBase(ITestOutputHelper testOutputHelper) =>
        TestOutputHelper = testOutputHelper;

    public string StepName => typeof(TStep).GetDisplayName();

    /// <inheritdoc />
    public Type StepType => typeof(TStep);

    public ITestOutputHelper TestOutputHelper { get; }

    [Fact]
    public void All_Properties_should_have_distinct_consecutive_positive_orders()
    {
        var instance = new TStep();
        var errors   = new List<string>();

        var properties = typeof(TStep).GetProperties()
            .Select(
                propertyInfo =>
                    (propertyInfo,
                     attribute: propertyInfo.GetCustomAttribute<StepPropertyBaseAttribute>())
            )
            .Where(x => x.attribute != null)
            .OrderBy(x => x.attribute!.Order)
            .ToList();

        var expectedI     = 1;
        var canBeRequired = true;

        foreach (var (propertyInfo, attribute) in properties)
        {
            var propName = $"{typeof(TStep).GetDisplayName()}.{propertyInfo.Name}";
            var required = propertyInfo.IsDecoratedWith<RequiredAttribute>();

            if (required)
            {
                if (!canBeRequired)
                    errors.Add(
                        $"{propName} should not be required as it comes after non-required attributes"
                    );
            }
            else
                canBeRequired = false; //Required attributes cannot follow non-required attributes

            if (attribute!.Order != expectedI)
                errors.Add($"{propName} has order {attribute.Order} but should have {expectedI}");

            expectedI++;
        }

        if (errors.Any())
            throw new XunitException(string.Join("\r\n", errors));
    }

    [Fact]
    public void All_properties_should_be_required_or_have_default_values_and_attributes()
    {
        var instance = new TStep();
        var errors   = new List<string>();

        foreach (var propertyInfo in typeof(TStep).GetProperties()
            .Where(x => x.IsDecoratedWith<StepPropertyBaseAttribute>()))
        {
            var propName = $"{typeof(TStep).GetDisplayName()}.{propertyInfo.Name}";

            var required = propertyInfo.IsDecoratedWith<RequiredAttribute>();

            var hasDefaultAttribute =
                propertyInfo.IsDecoratedWith<DefaultValueExplanationAttribute>();

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
                    errors.Add(
                        $"{propName} has both required and defaultValueExplanation attributes"
                    );

                if (defaultValue != null && !(defaultValue is VariableName vn && vn == default))
                    errors.Add($"{propName} is required but it has a default value");
            }
            else if (hasDefaultAttribute)
            {
                if (propertyInfo.CustomAttributes.All(
                        x => x.AttributeType.Name != "NullableAttribute"
                    ) &&
                    defaultValue == null)
                    errors.Add(
                        $"{propName} has a default value explanation but is not nullable and it's default value is null"
                    );
            }
            else
            {
                errors.Add(
                    $"{propName} has neither required nor defaultValueExplanation attributes"
                );
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

        stepFactoryType.Name.Should().StartWith(typeof(TStep).Name.Trim('`', '1'));

        var constructor = stepFactoryType.GetConstructor(System.Array.Empty<Type>());

        constructor.Should()
            .BeNull($"{StepName} should not have a public parameterless constructor");

        var instanceProperty = stepFactoryType.GetProperty(
            "Instance",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty
        );

        instanceProperty.Should()
            .NotBeNull($"{StepName} should have a public static Get property named 'Instance'");

        instanceProperty.Should().NotBeWritable($"{StepName}.Instance should be readonly");

        instanceProperty!.PropertyType.Should()
            .BeAssignableTo<IStepFactory>($"{StepName}.Instance should return an IStepFactory");
    }
}

}
