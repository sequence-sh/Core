using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Methods to create freezable types
    /// </summary>
    public static class FreezableFactory
    {
        //TODO move other CreateFreezable methods here

        /// <summary>
        /// Create a freezable Not step.
        /// </summary>
        public static IFreezableStep CreateFreezableNot(IFreezableStep boolean, IErrorLocation location)
        {
            var dict = new Dictionary<string, FreezableStepProperty>
            {
                {nameof(Not.Boolean), new FreezableStepProperty(boolean, location)},
            };

            var fpd = new FreezableStepData(dict, location);
            var step = new CompoundFreezableStep(NotStepFactory.Instance.TypeName, fpd, null);

            return step;
        }


        /// <summary>
        /// Create a new Freezable Array
        /// </summary>
        public static IFreezableStep CreateFreezableList(ImmutableList<IFreezableStep> elements, Configuration? configuration, IErrorLocation location)
        {
            if (elements.Any() && elements
                .All(x => x is CreateEntityFreezableStep || x is ConstantFreezableStep cfs && cfs.Value.IsT6))
            {
                var dict = new Dictionary<string, FreezableStepProperty>
                {
                    {nameof(EntityStreamCreate.Elements), new FreezableStepProperty(elements, location)}
                };

                var fpd = new FreezableStepData(dict, location);

                return new CompoundFreezableStep(EntityStreamCreateStepFactory.Instance.TypeName, fpd, configuration);
            }
            else
            {
                var dict = new Dictionary<string, FreezableStepProperty>
                {
                    {nameof(Array<object>.Elements), new FreezableStepProperty(elements, location)}
                };

                var fpd = new FreezableStepData(dict, location);

                return new CompoundFreezableStep(ArrayStepFactory.Instance.TypeName, fpd, configuration);
            }
        }

    }

    public static class InfixHelper
    {
        private class OperatorData
        {
            public OperatorData(string stepName, string leftName, string rightName, string operatorStepName,
                IFreezableStep operatorStep)
            {
                LeftName = leftName;
                RightName = rightName;
                OperatorStepName = operatorStepName;
                OperatorStep = operatorStep;
                StepName = stepName;
            }

            public string StepName { get; }

            public string LeftName { get; }

            public string RightName { get; }

            public string OperatorStepName { get; }

            public IFreezableStep OperatorStep { get; }
        }

        /// <summary>
        /// Creates an infix operator step
        /// </summary>
        public static Result<FreezableStepProperty, IError> TryCreateStep(
            IErrorLocation errorLocation,
            Result<FreezableStepProperty, IError> left,
            Result<FreezableStepProperty, IError> right, string op)
        {

            List<IError> errors = new List<IError>();

            if (left.IsFailure) errors.Add(left.Error);
            if (right.IsFailure) errors.Add(right.Error);

            if (!OperatorDataDictionary.TryGetValue(op, out var opData))
                errors.Add(new SingleError($"Operator '{op}' is not defined", ErrorCode.CouldNotParse, errorLocation));


            if (errors.Any())
                return Result.Failure<FreezableStepProperty, IError>(ErrorList.Combine(errors));

            var data = new FreezableStepData(new Dictionary<string, FreezableStepProperty>
            {
                {opData!.OperatorStepName, new FreezableStepProperty(opData.OperatorStep, errorLocation )},
                {opData.LeftName, left.Value},
                {opData.RightName, right.Value},

            }, errorLocation);

            var step = new CompoundFreezableStep(opData.StepName, data, null);

            return new FreezableStepProperty(step, errorLocation);

        }



        private static readonly IReadOnlyDictionary<string, OperatorData> OperatorDataDictionary =
            Enum.GetValues<MathOperator>()

                .ToDictionary(mo => mo.GetDisplayName(), mo => new OperatorData(
                    nameof(ApplyMathOperator),
                    nameof(ApplyMathOperator.Left),
                    nameof(ApplyMathOperator.Right),
                    nameof(ApplyMathOperator.Operator),
                    new ConstantFreezableStep(new Enumeration(nameof(MathOperator), mo.ToString()))
                ))
                .Concat(
                    Enum.GetValues<BooleanOperator>()
                .ToDictionary(mo => mo.GetDisplayName(), mo => new OperatorData(
                    nameof(ApplyBooleanOperator),
                    nameof(ApplyBooleanOperator.Left),
                    nameof(ApplyBooleanOperator.Right),
                    nameof(ApplyBooleanOperator.Operator),
                    new ConstantFreezableStep(new Enumeration(nameof(BooleanOperator), mo.ToString()))
                ))
                    )
                .Concat(
                    Enum.GetValues<CompareOperator>()
                .ToDictionary(mo => mo.GetDisplayName(), mo => new OperatorData(
                    CompareStepFactory.Instance.TypeName,
                    nameof(Compare<int>.Left),
                    nameof(Compare<int>.Right),
                    nameof(Compare<int>.Operator),
                    new ConstantFreezableStep(new Enumeration(nameof(CompareOperator), mo.ToString()))
                ))
                    )
                .Where(x => x.Key != MathOperator.None.ToString())

                .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);


    }
}
