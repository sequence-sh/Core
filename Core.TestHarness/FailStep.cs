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
        protected static TStep CreateStepWithFailStepsAsValues()
        {
            var instance = new TStep();


            foreach (var propertyInfo in typeof(TStep).GetProperties()
                .Where(x => x.IsDecoratedWith<StepPropertyBaseAttribute>()).OrderBy(x => x.Name))
            {
                MatchStepPropertyInfo(propertyInfo, SetVariableName, SetStep, SetStepList);
            }

            return instance;


            void SetVariableName(PropertyInfo property)
            {
                var vn = new VariableName("Foo");
                property.SetValue(instance, vn);
            }

            void SetStep(PropertyInfo property)
            {
                var newValue = CreateFailStepOfType(property.PropertyType.GenericTypeArguments.First());
                property.SetValue(instance, newValue);
            }

            void SetStepList(PropertyInfo property)
            {
                var newValue = CreateFailStepListOfElementType(property.PropertyType.GenericTypeArguments.First(), 1);
                property.SetValue(instance, newValue);
            }
        }

        protected static IEnumerable<IStep> CreateFailStepListOfElementType(Type stepType, int numberOfElements)
        {
            var listType = typeof(List<>).MakeGenericType(stepType);
            var list = Activator.CreateInstance(listType)!;

            var addMethod = list.GetType().GetMethod(nameof(List<object>.Add))!;

            var elementType = stepType.GenericTypeArguments.First();

            for (var i = 0; i < numberOfElements; i++)
            {
                var step = CreateFailStepOfType(elementType);
                addMethod.Invoke(list, new object[]{step});
            }

            return (IEnumerable<IStep>) list;
        }

        protected static IStep CreateFailStepOfType(Type outputType)
        {
            var fsType = typeof(FailStep<>).MakeGenericType(typeof(TStep), typeof(TOutput), outputType);

            var instance = Activator.CreateInstance(fsType);

            var step = (IStep) instance!;

            return step;
        }

        /// <summary>
        /// The error returned by a FailStep
        /// </summary>
        protected static IError FailStepError { get; } = new SingleError("Intentional Test Error", ErrorCode.Test, EntireSequenceLocation.Instance);

        private class FailStep<T> : CompoundStep<T>
        {
            /// <inheritdoc />
            public override async Task<Result<T, IError>> Run(StateMonad stateMonad,
                CancellationToken cancellationToken)
            {
                await Task.CompletedTask;

                return Result.Failure<T, IError>(FailStepError);
            }

            /// <inheritdoc />
            public override IStepFactory StepFactory => FailStepFactory.Instance;

            private class FailStepFactory : SimpleStepFactory<FailStep<T>, T>
            {
                private FailStepFactory()
                {
                }

                public static SimpleStepFactory<FailStep<T>, T> Instance { get; } = new FailStepFactory();
            }
        }
    }
}
