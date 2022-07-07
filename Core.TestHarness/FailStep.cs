namespace Reductech.Sequence.Core.TestHarness;

public abstract partial class StepTestBase<TStep, TOutput>
{
    protected static
        IEnumerable<(TStep step, IError expectedError, IReadOnlyDictionary<VariableName, ISCLObject>
            variablesToInject)>
        CreateStepsWithFailStepsAsValues(ISCLObject? defaultVariableValue)
    {
        var allProperties = typeof(TStep).GetProperties()
            .Where(x => x.GetCustomAttribute<StepPropertyBaseAttribute>() is not null)
            .OrderBy(x => x.Name)
            .ToList();

        foreach (var propertyToFail in allProperties)
        {
            if (defaultVariableValue == null && propertyToFail.PropertyType == typeof(VariableName))
                continue; //Don't bother testing this

            var     instance          = new TStep();
            var     variablesToInject = new Dictionary<VariableName, ISCLObject>();
            IError? error             = null;

            foreach (var propertyInfo in allProperties)
            {
                if (propertyInfo == propertyToFail)
                {
                    error =
                        MatchStepPropertyInfo(
                            propertyInfo,
                            x =>
                            {
                                SetVariableName(x, instance);

                                return
                                    new SingleError(
                                        new ErrorLocation(instance),
                                        ErrorCode.MissingVariable,
                                        "<Foo>"
                                    );
                            },
                            x => SetFailStep(x, instance),
                            x => SetFailLambda(x, instance),
                            x => SetFailStepList(x, instance)
                        );
                }
                else
                {
                    var variableWasSet =
                        MatchStepPropertyInfo(
                            propertyInfo,
                            x => SetVariableName(x, instance),
                            x => SetStep(x, instance),
                            x => SetLambda(x, instance),
                            x => SetStepList(x, instance)
                        );

                    if (variableWasSet && defaultVariableValue != null)
                    {
                        variablesToInject.Add(
                            FooVariableName(),
                            defaultVariableValue
                        );
                    }
                }
            }

            if (error == null)
                throw new Exception("error was not set in fail step creation");

            yield return (instance, error, variablesToInject);
        }

        static VariableName FooVariableName() => new("Foo");

        static bool SetVariableName(PropertyInfo property, object instance)
        {
            property.SetValue(instance, FooVariableName());

            return true;
        }

        static bool SetStep(PropertyInfo property, object instance)
        {
            var (step, _, _) = CreateSimpleStep(property, 1);
            property.SetValue(instance, step);

            return false;
        }

        static bool SetLambda(PropertyInfo property, object instance)
        {
            if (!property.PropertyType.GenericTypeArguments.Any())
                throw new Exception($"Property {property} has no generic arguments");

            var (step, _, _) = CreateSimpleStep(
                property.PropertyType.GenericTypeArguments[1],
                property.Name,
                1,
                false
            );

            var lambda = Activator.CreateInstance(property.PropertyType, null, step);

            property.SetValue(instance, lambda);

            return false;
        }

        static bool SetStepList(PropertyInfo property, object instance)
        {
            var i = 0;

            var newValue = CreateStepListOfType(
                property.PropertyType.GenericTypeArguments.First(),
                1,
                ref i
            );

            property.SetValue(instance, newValue);
            return false;
        }

        static IError SetFailLambda(PropertyInfo property, object instance)
        {
            var errorMessage = property.Name + " Error";

            var step = CreateFailStepOfType(
                property.PropertyType.GenericTypeArguments[1],
                errorMessage
            );

            try
            {
                var lambda = Activator.CreateInstance(property.PropertyType, null, step);
                property.SetValue(instance, lambda);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return new SingleError(
                ErrorLocation.EmptyLocation,
                ErrorCode.Test,
                errorMessage
            );
        }

        static IError SetFailStep(PropertyInfo property, object instance)
        {
            var errorMessage = property.Name + " Error";

            var newValue = CreateFailStepOfType(
                property.PropertyType.GenericTypeArguments.FirstOrDefault(typeof(ISCLObject)),
                errorMessage
            );

            property.SetValue(instance, newValue);

            return new SingleError(
                ErrorLocation.EmptyLocation,
                ErrorCode.Test,
                errorMessage
            );
        }

        static IError SetFailStepList(PropertyInfo property, object instance)
        {
            var errorMessage = property.Name + " Error";

            var newValue = CreateFailStepListOfElementType(
                property.PropertyType.GenericTypeArguments.First(),
                errorMessage
            );

            property.SetValue(instance, newValue);

            return new SingleError(
                ErrorLocation.EmptyLocation,
                ErrorCode.Test,
                errorMessage
            );
        }
    }

    protected static IEnumerable<IStep> CreateFailStepListOfElementType(
        Type stepType,
        string errorMessage)
    {
        var listType = typeof(List<>).MakeGenericType(stepType);
        var list     = Activator.CreateInstance(listType)!;

        var addMethod = list.GetType().GetMethod(nameof(List<object>.Add))!;

        var elementType = stepType.IsGenericType
            ? stepType.GenericTypeArguments.First()
            : typeof(ISCLObject);

        var step = CreateFailStepOfType(elementType, errorMessage);
        addMethod.Invoke(list, new object[] { step });

        return (IEnumerable<IStep>)list;
    }

    protected static IStep CreateFailStepOfType(Type outputType, string errorMessage)
    {
        var fsType = typeof(FailStep<>).MakeGenericType(typeof(TStep), typeof(TOutput), outputType);

        var instance = Activator.CreateInstance(fsType);

        var step = (IStep)instance!;

        var errorMessageProperty =
            step.GetType().GetProperty(nameof(FailStep<ISCLObject>.ErrorMessage));

        errorMessageProperty!.SetValue(step, errorMessage);

        return step;
    }

    public class FailStep<T> : CompoundStep<T> where T : ISCLObject
    {
        /// <inheritdoc />
        protected override async Task<Result<T, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var error = new SingleError(
                ErrorLocation.EmptyLocation,
                ErrorCode.Test,
                ErrorMessage
            );

            return error;
        }

        /// <summary>
        /// The error message that will be returned
        /// </summary>
        public string ErrorMessage { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => FailStepFactory.Instance;

        private class FailStepFactory : SimpleStepFactory<FailStep<T>, T>
        {
            private FailStepFactory() { }

            public static SimpleStepFactory<FailStep<T>, T> Instance { get; } =
                new FailStepFactory();
        }
    }
}
