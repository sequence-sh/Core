using System.IO;

namespace Reductech.EDR.Core.TestHarness;

public abstract partial class StepTestBase<TStep, TOutput>
{
    protected static (TStep step, Dictionary<string, string> values)
        CreateStepWithDefaultOrArbitraryValues()
    {
        var instance = new TStep();

        var index = 0;

        var values = new Dictionary<string, string>();

        foreach (var propertyInfo in typeof(TStep).GetProperties()
                     .Select(
                         propertyInfo => (propertyInfo,
                                          attribute: propertyInfo
                                              .GetCustomAttribute<StepPropertyBaseAttribute>())
                     )
                     .Where(x => x.attribute != null)
                     .OrderByDescending(x => x.attribute!.Order != null)
                     .ThenBy(x => x.attribute!.Order)
                     .Select(x => x.propertyInfo)
                )
            MatchStepPropertyInfo(propertyInfo, SetVariableName, SetStep, SetLambda, SetStepList);

        return (instance, values);

        Unit SetVariableName(PropertyInfo property)
        {
            var vn = new VariableName("Foo" + index);
            index++;
            property.SetValue(instance, vn);
            values.Add(property.Name, $"<{vn.Name}>");

            return Unit.Default;
        }

        Unit SetStep(PropertyInfo property)
        {
            var currentValue = property.GetValue(instance);

            if (currentValue == null)
            {
                var (step, value, newIndex) = CreateSimpleStep(property, index);
                index                       = newIndex;
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
                var s = ((IStep)currentValue).Serialize();
                values.Add(property.Name, s);
            }

            return Unit.Default;
        }

        Unit SetLambda(PropertyInfo property)
        {
            var currentValue = property.GetValue(instance);

            if (currentValue == null)
            {
                var (step, value, newIndex) = CreateSimpleStep(
                    property.PropertyType.GenericTypeArguments[1],
                    property.Name,
                    index,
                    false
                );

                index = newIndex;
                values.Add(property.Name, $"(<> => {value})");

                try
                {
                    var lambda = Activator.CreateInstance(
                        property.PropertyType,
                        null as VariableName?,
                        step
                    );

                    property.SetValue(instance, lambda);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            else
            {
                var s = ((LambdaFunction)currentValue).Serialize();
                values.Add(property.Name, s);
            }

            return Unit.Default;
        }

        Unit SetStepList(PropertyInfo property)
        {
            if (property.GetValue(instance) is IReadOnlyList<IStep> currentValue)
            {
                var elements = new List<string>();

                foreach (var step in currentValue)
                {
                    var s = step.Serialize();
                    elements.Add(s);
                }

                var currentValueString = CreateArrayString(elements);

                values.Add(property.Name, currentValueString);
            }
            else
            {
                var newValue = CreateStepListOfType(property.PropertyType, 3, ref index);
                var elements = new List<string>();

                foreach (var step in newValue)
                {
                    var e = step.Serialize();
                    elements.Add(e);
                }

                var newValueString = CreateArrayString(elements);

                values.Add(property.Name, newValueString);
                property.SetValue(instance, newValue);
            }

            static string CreateArrayString(IEnumerable<string> elements)
            {
                return $"[{string.Join(", ", elements)}]";
            }

            return Unit.Default;
        }
    }

    private static T MatchStepPropertyInfo<T>(
        PropertyInfo stepPropertyInfo,
        Func<PropertyInfo, T> variableNameAction,
        Func<PropertyInfo, T> stepPropertyAction,
        Func<PropertyInfo, T> lambdaAction,
        Func<PropertyInfo, T> stepListAction)
    {
        var actions = new List<Func<PropertyInfo, T>>();

        if (stepPropertyInfo.GetCustomAttribute<VariableNameAttribute>() is not null)
            actions.Add(variableNameAction);

        if (stepPropertyInfo.GetCustomAttribute<StepPropertyAttribute>() is not null)
            actions.Add(stepPropertyAction);

        if (stepPropertyInfo.GetCustomAttribute<StepListPropertyAttribute>() is not null)
            actions.Add(stepListAction);

        if (stepPropertyInfo.GetCustomAttribute<FunctionPropertyAttribute>() is not null)
            actions.Add(lambdaAction);

        return actions.Count switch
        {
            0 => throw new XunitException(
                $"{stepPropertyInfo.Name} does not have a valid attribute"
            ),
            1 => actions.Single()(stepPropertyInfo),
            _ => throw new XunitException(
                $"{stepPropertyInfo.Name} has more than one step property base attribute"
            )
        };
    }

    private static (IStep step, string value, int newIndex) CreateSimpleStep(
        PropertyInfo propertyInfo,
        int index)
    {
        var tStep = propertyInfo.PropertyType;

        var outputType = tStep.GenericTypeArguments.First();

        var singleChar = propertyInfo.GetCustomAttribute<SingleCharacterAttribute>() != null;

        return CreateSimpleStep(outputType, tStep.Name, index, singleChar);
    }

    private static (IStep step, string value, int newIndex) CreateSimpleStep(
        Type outputType,
        string stepName,
        int index,
        bool singleChar)
    {
        IStep step;

        if (outputType == typeof(Unit))
        {
            step = new DoNothing();
        }

        else if (outputType == typeof(StringStream))
        {
            string s;

            if (singleChar)
                s = "" + index;
            else
                s = "Bar" + index;

            index++;
            step = Constant(s);
        }
        else if (outputType == typeof(string))
        {
            throw new Exception(
                $"{stepName} should not have output type 'String' - it should be 'StringStream'"
            );
        }

        else if (outputType == typeof(bool))
        {
            var b = true;
            step = Constant(b);
        }
        else if (outputType == typeof(int))
        {
            var i = index;
            index++;
            step = Constant(i);
        }

        else if (outputType == typeof(double))
        {
            double d = index;
            index++;
            step = Constant(d);
        }
        else if (outputType == typeof(DateTime))
        {
            var dt = new DateTime(1990, index, 1);
            index++;
            step = Constant(dt);
        }

        else if (outputType == typeof(Array<StringStream>))
        {
            var list = new List<string>();

            for (var i = 0; i < 3; i++)
            {
                list.Add("Foo" + index);
                index++;
            }

            step = Array(list.ToArray());
        }
        else if (outputType == typeof(Array<int>))
        {
            step = CreateIntArray(ref index);
        }
        else if (outputType == typeof(Array<double>))
        {
            var list = new List<double>();

            for (var i = 0; i < 3; i++)
            {
                list.Add(index * 1.1);
                index++;
            }

            step = Array(list.ToArray());
        }
        else if (outputType == typeof(Array<bool>))
        {
            var list = new List<bool>();

            for (var i = 0; i < 3; i++)
            {
                list.Add(i % 2 == 0);
                index++;
            }

            step = Array(list.ToArray());
        }
        else if (outputType == typeof(Array<Entity>))
        {
            var entityStream = CreateSimpleEntityStream(ref index);

            step = entityStream;
        }
        else if (outputType == typeof(Array<Array<Entity>>))
        {
            var entityStreamList = new List<IStep<Array<Entity>>>
            {
                CreateSimpleEntityStream(ref index),
                CreateSimpleEntityStream(ref index),
                CreateSimpleEntityStream(ref index)
            };

            step = new ArrayNew<Array<Entity>> { Elements = entityStreamList };
        }
        else if (outputType == typeof(Array<Array<int>>))
        {
            var intArrayList = new List<IStep<Array<int>>>
            {
                CreateIntArray(ref index), CreateIntArray(ref index), CreateIntArray(ref index),
            };

            step = new ArrayNew<Array<int>> { Elements = intArrayList };
        }

        else if (outputType.IsEnum)
        {
            var v = Enum.GetValues(outputType).OfType<object>().First();

            step = EnumConstantFreezable.TryCreateEnumConstant(v).Value;
        }

        else if (outputType == typeof(Stream))
        {
            throw new Exception(
                $"{stepName} should not have output type 'Stream' - it should be 'StringStream'"
            );
        }
        else if (outputType == typeof(Entity))
        {
            var entity = CreateSimpleEntity(ref index);

            step = Constant(entity);
        }
        else if (outputType == typeof(StringStream))
        {
            var s = "DataStream" + index;
            index++;

            step = new StringConstant(s);
        }
        else if (outputType.IsGenericType && outputType.GetInterfaces().Contains(typeof(IOneOf)))
        {
            var arg1 = outputType.GenericTypeArguments[0];
            var (step1, _, newIndex) = CreateSimpleStep(arg1, stepName, index, singleChar);
            index                    = newIndex;

            step = OneOfStep.Create(outputType, step1);
        }
        else
            throw new XunitException(
                $"Cannot create a constant step with type {outputType.GetDisplayName()}"
            );

        var newString = step.Serialize(); // await GetStringAsync(step);

        return (step, newString, index);

        static IStep<Array<Entity>> CreateSimpleEntityStream(ref int index1)
        {
            var entityList = new List<IStep<Entity>>
            {
                Constant(CreateSimpleEntity(ref index1)),
                Constant(CreateSimpleEntity(ref index1)),
                Constant(CreateSimpleEntity(ref index1))
            };

            var entityStream = new ArrayNew<Entity> { Elements = entityList };

            return entityStream;
        }

        static Entity CreateSimpleEntity(ref int index1)
        {
            var pairs =
                new List<(EntityPropertyKey, object?)>
                {
                    (new EntityPropertyKey("Prop1"), $"Val{index1}")
                };

            index1++;
            pairs.Add((new EntityPropertyKey("Prop2"), $"Val{index1}"));
            index1++;

            var entity = Entity.Create(pairs);
            return entity;
        }

        static IStep<Array<int>> CreateIntArray(ref int index)
        {
            var list = new List<int>();

            for (var i = 0; i < 3; i++)
            {
                list.Add(index);
                index++;
            }

            return Array(list.ToArray());
        }
    }

    private static IEnumerable<IStep> CreateStepListOfType(
        Type tList,
        int numberOfElements,
        ref int index)
    {
        Func<int, object> getElement;

        Type stepType = !tList.GenericTypeArguments.First().IsGenericType
            ? typeof(IStep<Unit>)
            : tList.GenericTypeArguments.First();

        var outputType = stepType.GenericTypeArguments.First();

        if (outputType == typeof(Unit))
            getElement = _ => new DoNothing();

        else if (outputType == typeof(string))
            getElement = i => Constant("Bar" + i);

        else if (outputType == typeof(bool))
            getElement = i => Constant(i % 2 == 0);
        else if (outputType == typeof(int))
            getElement = Constant;
        else if (outputType == typeof(Entity))
            getElement = i => Constant(Entity.Create(("Foo", $"Val + {i}")));
        else
            throw new XunitException(
                $"Cannot create default value for {outputType.GetDisplayName()} List"
            );

        var listType  = typeof(List<>).MakeGenericType(stepType);
        var list      = Activator.CreateInstance(listType)!;
        var addMethod = list.GetType().GetMethod(nameof(List<object>.Add))!;

        for (var i = 0; i < numberOfElements; i++)
        {
            var e = getElement(index);
            index++;
            addMethod.Invoke(list, new[] { e });
        }

        return (IEnumerable<IStep>)list;
    }
}
