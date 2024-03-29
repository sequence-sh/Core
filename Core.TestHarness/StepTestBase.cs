﻿namespace Sequence.Core.TestHarness;

/// <summary>
/// A test that tests the behaviour of a step
/// </summary>
public interface IStepTestBase
{
    /// <summary>
    /// The name ofd the step
    /// </summary>
    string StepName { get; }

    /// <summary>
    /// The Type of the step
    /// </summary>
    Type StepType { get; }
}

/// <summary>
/// A test that tests the behaviour of a step
/// </summary>
[UseTestOutputHelper]
public abstract partial class StepTestBase<TStep, TOutput> : IStepTestBase
    where TStep : class, ICompoundStep<TOutput>, new()
    where TOutput : ISCLObject
{
    /// <inheritdoc />
    public string StepName => typeof(TStep).GetDisplayName();

    /// <inheritdoc />
    public Type StepType => typeof(TStep);

    /// <summary>
    /// Checks that all properties have the correct attributes
    /// </summary>
    [Fact]
    public void All_Properties_should_have_correct_attribute()
    {
        var properties = typeof(TStep).GetProperties()
            .Select(
                propertyInfo =>
                    (propertyInfo,
                     attribute: propertyInfo.GetCustomAttribute<StepPropertyBaseAttribute>())
            )
            .Where(x => x.attribute != null)
            .ToList();

        foreach (var (propertyInfo, attribute) in properties)
        {
            switch (attribute)
            {
                case null: break;
                case FunctionPropertyAttribute _:
                    propertyInfo.PropertyType.Should().BeAssignableTo(typeof(LambdaFunction));
                    break;
                case StepListPropertyAttribute _:
                    propertyInfo.PropertyType.Should().BeAssignableTo(typeof(IReadOnlyList<>));
                    break;
                case StepPropertyAttribute _:
                    propertyInfo.PropertyType.Should().BeAssignableTo<IStep>();
                    break;
                case VariableNameAttribute _:
                    propertyInfo.PropertyType.Should().BeAssignableTo(typeof(VariableName));
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(attribute));
            }
        }
    }

    /// <summary>
    /// Checks property ordering
    /// </summary>
    [Fact]
    public void All_Properties_should_have_distinct_consecutive_positive_orders()
    {
        var errors = new List<string>();

        var properties = typeof(TStep).GetProperties()
            .Select(
                propertyInfo =>
                    (propertyInfo,
                     attribute: propertyInfo.GetCustomAttribute<StepPropertyBaseAttribute>())
            )
            .Where(x => x.attribute != null)
            .Where(x => x.attribute!.Order.HasValue)
            .OrderBy(x => x.attribute!.Order)
            .ToList();

        var expectedI     = 1;
        var canBeRequired = true;

        foreach (var (propertyInfo, attribute) in properties)
        {
            var propName = $"{typeof(TStep).GetDisplayName()}.{propertyInfo.Name}";
            var required = propertyInfo.GetCustomAttribute<RequiredAttribute>() is not null;

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

    /// <summary>
    /// Checks required and default values
    /// </summary>
    [Fact]
    public void All_properties_should_be_required_or_have_default_values_and_attributes()
    {
        var instance = new TStep();
        var errors   = new List<string>();

        foreach (var propertyInfo in typeof(TStep).GetProperties()
                     .Where(x => x.GetCustomAttribute<StepPropertyBaseAttribute>() is not null))
        {
            var propName = $"{typeof(TStep).GetDisplayName()}.{propertyInfo.Name}";

            var required = propertyInfo.GetCustomAttribute<RequiredAttribute>() is not null;

            var hasDefaultAttribute =
                propertyInfo.GetCustomAttribute<DefaultValueExplanationAttribute>() is not null;

            if (propertyInfo.SetMethod == null || !propertyInfo.SetMethod.IsPublic)
                errors.Add($"{propName} has no public setter");

            if (propertyInfo.GetMethod == null || !propertyInfo.GetMethod.IsPublic)
            {
                errors.Add($"{propName} has no public getter");
                continue;
            }

            var defaultValue = propertyInfo.GetValue(instance);

            if (hasDefaultAttribute)
            {
                if (defaultValue == null
                 && propertyInfo.ToContextualProperty().Nullability != Nullability.Nullable)
                    errors.Add(
                        $"{propName} has a default value explanation but is not nullable and it's default value is null"
                    );
            }
            else
            {
                if (required)
                {
                    if (defaultValue != null && !(defaultValue is VariableName vn && vn == default))
                        errors.Add($"{propName} is required but it has a default value");
                }
                else
                {
                    errors.Add(
                        $"{propName} has neither required nor defaultValueExplanation attributes"
                    );
                }
            }

            //if (required)
            //{
            //    if (hasDefaultAttribute)
            //        errors.Add(
            //            $"{propName} has both required and defaultValueExplanation attributes"
            //        );

            //}
            //else
            //{

            //}
        }

        if (errors.Any())
            throw new XunitException(string.Join("\r\n", errors));
    }

    /// <summary>
    /// Checks that the step factory is set correctly
    /// </summary>
    [Fact]
    public void Process_factory_must_be_set_correctly()
    {
        var instance = new TStep();

        instance.StepFactory.Should().NotBeNull();

        var stepType = (instance.StepFactory as StepFactory)!.StepType;

        stepType.Name.Should().Be(typeof(TStep).Name); //Compare type names because of generic types

        typeof(TStep).Should().BeAssignableTo(stepType);
    }
}
