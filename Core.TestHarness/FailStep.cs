using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions.Common;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.TestHarness
{
    public abstract partial class StepTestBase<TStep, TOutput>
    {
        protected static IEnumerable<(TStep step, IError expectedError, IReadOnlyCollection<Action<IStateMonad>>)> CreateStepsWithFailStepsAsValues(object? defaultVariableValue)
        {
            var allProperties = typeof(TStep).GetProperties()
                .Where(x => x.IsDecoratedWith<StepPropertyBaseAttribute>()).OrderBy(x => x.Name)
                .ToList();

            foreach (var propertyToFail in allProperties)
            {
                if(defaultVariableValue == null && propertyToFail.PropertyType ==typeof(VariableName))
                    continue;//Don't bother testing this

                var instance = new TStep();
                var stateActions = new List<Action<IStateMonad>>();
                IError? error = null;

                foreach (var propertyInfo in allProperties)
                {
                    if (propertyInfo == propertyToFail)
                    {
                        error =
                            MatchStepPropertyInfo(propertyInfo,
                                x=>
                                {
                                    SetVariableName(x, instance);
                                    return
                                        new SingleError("Variable '<Foo>' does not exist.", ErrorCode.MissingVariable,
                                            new StepErrorLocation(instance));
                                },
                                x=>SetFailStep(x, instance),
                                x=> SetFailStepList(x,instance));
                    }
                    else
                    {
                        var variableWasSet =
                            MatchStepPropertyInfo(propertyInfo,
                                x => SetVariableName(x, instance),
                                x => SetStep(x, instance),
                                x => SetStepList(x, instance));
                        if (variableWasSet && defaultVariableValue != null)
                        {
                            stateActions.Add(x=>x.SetVariable(FooVariableName(), defaultVariableValue));
                        }
                    }
                }
                if (error == null)
                    throw new Exception("error was not set in fail step creation");

                yield return (instance, error, stateActions);
            }

            static VariableName FooVariableName() => new VariableName("Foo");


            static bool SetVariableName(PropertyInfo property, object instance)
            {
                property.SetValue(instance, FooVariableName());

                return true;
            }

            static bool SetStep(PropertyInfo property, object instance)
            {
                if (!property.PropertyType.GenericTypeArguments.Any())
                    throw new Exception($"Property {property} has no generic arguments");


                var ssTask = CreateSimpleStep(property, 1);
                var newValue = ssTask.Result.step;
                property.SetValue(instance, newValue);

                return false;
            }

            static bool SetStepList(PropertyInfo property, object instance)
            {
                var i = 0;
                var newValue = CreateStepListOfType(property.PropertyType.GenericTypeArguments.First(), 1, ref i);
                property.SetValue(instance, newValue);
                return false;
            }

            static IError SetFailStep(PropertyInfo property, object instance)
            {
                var errorMessage = property.Name + " Error";

                var newValue = CreateFailStepOfType(property.PropertyType.GenericTypeArguments.First(), errorMessage);
                property.SetValue(instance, newValue);
                return new SingleError(errorMessage, ErrorCode.Test, EntireSequenceLocation.Instance);
            }

            static IError SetFailStepList(PropertyInfo property, object instance)
            {
                var errorMessage = property.Name + " Error";

                var newValue = CreateFailStepListOfElementType(property.PropertyType.GenericTypeArguments.First(), errorMessage);
                property.SetValue(instance, newValue);
                return new SingleError(errorMessage, ErrorCode.Test, EntireSequenceLocation.Instance);
            }
        }

        protected static IEnumerable<IStep> CreateFailStepListOfElementType(Type stepType, string errorMessage)
        {
            var listType = typeof(List<>).MakeGenericType(stepType);
            var list = Activator.CreateInstance(listType)!;

            var addMethod = list.GetType().GetMethod(nameof(List<object>.Add))!;

            var elementType = stepType.GenericTypeArguments.First();

            var step = CreateFailStepOfType(elementType, errorMessage);
            addMethod.Invoke(list, new object[] { step });

            return (IEnumerable<IStep>) list;
        }

        protected static IStep CreateFailStepOfType(Type outputType, string errorMessage)
        {
            var fsType = typeof(FailStep<>).MakeGenericType(typeof(TStep), typeof(TOutput), outputType);

            var instance = Activator.CreateInstance(fsType);

            var step = (IStep) instance!;

            var errorMessageProperty = step.GetType().GetProperty(nameof(FailStep<object>.ErrorMessage));

            errorMessageProperty!.SetValue(step, errorMessage);

            return step;
        }


        public const string IntentionalErrorString = "Intentional Error";

        public class FailStep<T> : CompoundStep<T>
        {
            /// <inheritdoc />
            public override async Task<Result<T, IError>> Run(IStateMonad stateMonad,
                CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                var error = new SingleError(ErrorMessage, ErrorCode.Test, EntireSequenceLocation.Instance);

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
                private FailStepFactory() {}

                public static SimpleStepFactory<FailStep<T>, T> Instance { get; } = new FailStepFactory();
            }
        }
    }
}
