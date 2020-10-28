using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions.Common;
using Namotion.Reflection;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Xunit.Sdk;

namespace Reductech.EDR.Core.TestHarness
{
    public abstract partial class StepTestBase<TStep, TOutput>
    {
        protected static (TStep step, Dictionary<string, string> values) CreateStepWithDefaultOrArbitraryValues()
        {
            var instance = new TStep();

            var index = 0;

            var values = new Dictionary<string, string>();

            foreach (var propertyInfo in typeof(TStep).GetProperties()
                .Where(x => x.IsDecoratedWith<StepPropertyBaseAttribute>()).OrderBy(x=>x.Name))
            {
                MatchStepPropertyInfo(propertyInfo, SetVariableName, SetStep, SetStepList);
            }

            return (instance, values);


            void SetVariableName(PropertyInfo property)
            {
                var vn = new VariableName("Foo" + index);
                index++;
                property.SetValue(instance, vn);
                values.Add(property.Name, $"<{vn.Name}>");
            }

            void SetStep(PropertyInfo property)
            {
                var currentValue = property.GetValue(instance);
                if (currentValue == null)
                {
                    var newValue = CreateConstantOfType(property.PropertyType, ref index);
                    values.Add(property.Name, newValue.Unfreeze().StepName);
                    property.SetValue(instance, newValue);
                }
                else
                {
                    values.Add(property.Name, (currentValue as IStep).Name);
                }
            }

            void SetStepList(PropertyInfo property)
            {
                var currentValue = property.GetValue(instance);
                if (currentValue == null)
                {
                    var newValue = CreateStepListOfType(property.PropertyType, 3, ref index);
                    var newValueString = CreateArrayString(newValue.Select(x => x.Unfreeze().StepName));

                    values.Add(property.Name, newValueString);
                    property.SetValue(instance, newValue);
                }
                else
                {
                    var currentValueString =
                        CreateArrayString(
                            (currentValue as IReadOnlyList<IStep>).Select(x => x.Unfreeze().StepName));

                    values.Add(property.Name, currentValueString);
                }

                static string CreateArrayString(IEnumerable<string> elements)
                {
                    return $"[{string.Join(", ", elements)}]";
                }
            }
        }

        private static void MatchStepPropertyInfo(PropertyInfo stepPropertyInfo,
            Action<PropertyInfo> variableNameAction,
            Action<PropertyInfo> stepPropertyAction,
            Action<PropertyInfo> stepListAction)
        {
            var actionsToDo = new List<Action<PropertyInfo>>();

            if (stepPropertyInfo.IsDecoratedWith<VariableNameAttribute>())
                actionsToDo.Add(variableNameAction);

            if (stepPropertyInfo.IsDecoratedWith<StepPropertyAttribute>())
                actionsToDo.Add(stepPropertyAction);

            if (stepPropertyInfo.IsDecoratedWith<StepListPropertyAttribute>())
                actionsToDo.Add(stepListAction);

            switch (actionsToDo.Count)
            {
                case 0: throw new XunitException($"{stepPropertyInfo.Name} does not have a valid attribute");
                case 1:
                    actionsToDo.Single()(stepPropertyInfo);
                    return;
                default:
                    throw new XunitException(
                        $"{stepPropertyInfo.Name} has more than one step property base attribute");
            }
        }

        private static IConstantStep CreateConstantOfType(Type tStep, ref int index)
        {
            var outputType = tStep.GenericTypeArguments.First();


            if (outputType == typeof(string))
            {
                var s = "Bar" + index;
                index++;

                return Constant(s);
            }
            else if (outputType == typeof(bool))
            {
                var b = true;
                return Constant(b);
            }
            else if (outputType == typeof(int))
            {
                var i = index;
                index++;
                return Constant(i);
            }
            else if (outputType == typeof(List<string>))
            {
                var list = new List<string>();
                for (var i = 0; i < 3; i++)
                {
                    list.Add("Foo" + index);
                    index++;
                }

                return Constant(list);
            }

            throw new XunitException($"Cannot create a constant step with type {outputType.GetDisplayName()}");
        }

        private static IReadOnlyList<IStep> CreateStepListOfType(Type tStep, int numberOfElements, ref int index)
        {
            var outputType = tStep.GenericTypeArguments.First();


            if (outputType == typeof(string))
            {
                var l = new List<IStep<string>>();

                for (var i = 0; i < numberOfElements; i++)
                {
                    var s = "Bar" + index;
                    index++;

                    l.Add(Constant(s));
                }

                return l;
            }
            else if (outputType == typeof(bool))
            {
                var l = new List<IStep<bool>>();

                for (var i = 0; i < numberOfElements; i++)
                {
                    var b = i % 2 == 0;

                    l.Add(Constant(b));
                }

                return l;
            }
            else if (outputType == typeof(int))
            {
                var l = new List<IStep<int>>();

                for (var i = 0; i < numberOfElements; i++)
                {
                    l.Add(Constant(index));
                    index++;
                }

                return l;
            }

            throw new XunitException($"Cannot create default value for {outputType.GetDisplayName()}");
        }
    }
}
