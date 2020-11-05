using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using FluentAssertions.Common;
using Namotion.Reflection;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;
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
                MatchStepPropertyInfo(propertyInfo, SetVariableName, SetStep, SetStepList);

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
                    var (step, value) = CreateSimpleStep(property.PropertyType, ref index);
                    values.Add(property.Name, value);

                    try
                    {
                        property.SetValue(instance, step);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
                else
                {
                    values.Add(property.Name, GetString((IStep) currentValue));
                }
            }

            void SetStepList(PropertyInfo property)
            {
                var currentValue = property.GetValue(instance);
                if (currentValue == null)
                {
                    var newValue = CreateStepListOfType(property.PropertyType, 3, ref index);
                    var newValueString = CreateArrayString(newValue.Select(GetString));

                    values.Add(property.Name, newValueString);
                    property.SetValue(instance, newValue);
                }
                else
                {
                    var currentValueString =
                        CreateArrayString(
                            (currentValue as IReadOnlyList<IStep>).Select(GetString));

                    values.Add(property.Name, currentValueString);
                }

                static string CreateArrayString(IEnumerable<string> elements)
                {
                    return $"[{string.Join(", ", elements)}]";
                }
            }
        }

        private static string GetString(IStep step)
        {
            var freezable = step.Unfreeze();

            if (freezable is ConstantFreezableStep cfs)
            {
                if (cfs.Value is Stream)
                {
                    throw new SerializationException("Cannot get string from a stream as that would enumerate the stream");
                    //return SerializationMethods.StreamToString(stream, Encoding.UTF8);
                }

                return ConstantFreezableStep.WriteValue(cfs.Value, true);
            }

            return freezable.StepName;
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

        private static (IStep step, string value) CreateSimpleStep(Type tStep, ref int index)
        {
            var outputType = tStep.GenericTypeArguments.First();
            IStep step;

            if (outputType == typeof(Unit))
            {
                step = new DoNothing();
            }

            else if (outputType == typeof(string))
            {
                var s = "Bar" + index;
                index++;
                step = Constant(s);
            }

            else if (outputType == typeof(bool))
            {
                var b = true;
                step =  Constant(b);
            }
            else if (outputType == typeof(int))
            {
                var i = index;
                index++;
                step =  Constant(i);
            }

            else if (outputType == typeof(double))
            {
                double d = index;
                index++;
                step =  Constant(d);
            }

            else if (outputType == typeof(List<string>))
            {
                var list = new List<string>();
                for (var i = 0; i < 3; i++)
                {
                    list.Add("Foo" + index);
                    index++;
                }

                step =  Constant(list);
            }
            else if (outputType == typeof(List<int>))
            {
                var list = new List<int>();
                for (var i = 0; i < 3; i++)
                {
                    list.Add(index);
                    index++;
                }

                step =  Constant(list);
            }

            else if (outputType.IsEnum)
            {
                var v = Enum.GetValues(outputType).OfType<object>().First();

                var constantType = typeof(Constant<>).MakeGenericType(outputType);
                var constant = Activator.CreateInstance(constantType, new[] {v});

                step =  (IStep) constant!;
            }

            else if (outputType == typeof(Stream))
            {
                var s = "Baz" + index;
                index++;

                byte[] byteArray = Encoding.UTF8.GetBytes(s);
                Stream stream = new MemoryStream(byteArray); //special case so we don't read the stream early

                step = Constant(stream);
                var asString = GetString(Constant(s));

                return (step, asString);
            }
            else
                throw new XunitException($"Cannot create a constant step with type {outputType.GetDisplayName()}");

            return (step, GetString(step));


        }


        private static IEnumerable<IStep> CreateStepListOfType(Type tList, int numberOfElements, ref int index)
        {
            var stepType = tList.GenericTypeArguments.First();
            var outputType = stepType.GenericTypeArguments.First();

            var listType = typeof(List<>).MakeGenericType(stepType);
            var list = Activator.CreateInstance(listType)!;

            var addMethod = list.GetType().GetMethod(nameof(List<object>.Add))!;

            Func<int, object> getElement;


            if (outputType == typeof(Unit)) getElement = _ => new DoNothing();


            else if (outputType == typeof(string)) getElement = i => Constant("Bar" + i);

            else if (outputType == typeof(bool)) getElement = i => Constant(i % 2 == 0);
            else if (outputType == typeof(int)) getElement = Constant;
            else throw new XunitException($"Cannot create default value for {outputType.GetDisplayName()}");

            for (var i = 0; i < numberOfElements; i++)
            {
                var e = getElement(index);
                index++;
                addMethod.Invoke(list, new[]{e});
            }

            return (IEnumerable<IStep>) list;

        }
    }
}
