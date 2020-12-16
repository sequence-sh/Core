using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;
using StepParameterDict = System.Collections.Generic.Dictionary<Reductech.EDR.Core.Internal.StepParameterReference, Reductech.EDR.Core.Internal.FreezableStepProperty>;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Methods to create freezable types
    /// </summary>
    public static class FreezableFactory
    {
        //TODO move other CreateFreezable methods here

        /// <summary>
        /// Create a new Freezable Sequence
        /// </summary>
        public static IFreezableStep CreateFreezableSequence(IEnumerable<IFreezableStep> steps, IFreezableStep finalStep, Configuration? configuration, IErrorLocation location)
        {
            var dict = new StepParameterDict
            {
                {new StepParameterReference(nameof(Sequence<object>.InitialSteps)), new FreezableStepProperty(steps.ToImmutableList(), location )},
                {new StepParameterReference(nameof(Sequence<object>.FinalStep)), new FreezableStepProperty(finalStep, location )},
            };

            var fpd = new FreezableStepData(dict, location);


            return new CompoundFreezableStep(SequenceStepFactory. Instance.TypeName, fpd, configuration);
        }

        /// <summary>
        /// Create a freezable GetVariable step.
        /// </summary>
        public static IFreezableStep CreateFreezableGetVariable(VariableName variableName, IErrorLocation location)
        {
            var dict = new StepParameterDict
            {
                {
                    new StepParameterReference(nameof(GetVariable<object>.Variable)), new FreezableStepProperty(variableName, location)
                }
            };

            var fpd = new FreezableStepData(dict, location);
            var step = new CompoundFreezableStep(GetVariableStepFactory.Instance.TypeName, fpd, null);

            return step;
        }

        /// <summary>
        /// Create a freezable GetVariable step.
        /// </summary>
        public static IFreezableStep CreateFreezableSetVariable(FreezableStepProperty variableName, FreezableStepProperty value, IErrorLocation location)
        {
            var dict = new StepParameterDict
            {
                {
                    new StepParameterReference(nameof(SetVariable<object>.Variable)), variableName
                },
                {
                    new StepParameterReference(nameof(SetVariable<object>.Value)), value
                },
            };

            var fpd = new FreezableStepData(dict, location);
            var step = new CompoundFreezableStep(SetVariableStepFactory.Instance.TypeName, fpd, null);

            return step;
        }


        /// <summary>
        /// Create a freezable Not step.
        /// </summary>
        public static IFreezableStep CreateFreezableNot(IFreezableStep boolean, IErrorLocation location)
        {
            var dict = new StepParameterDict
            {
                {new StepParameterReference(nameof(Not.Boolean)), new FreezableStepProperty(boolean, location)},
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

            var dict = new StepParameterDict
            {
                {
                    new StepParameterReference(nameof(Array<object>.Elements)),
                    new FreezableStepProperty(elements, location)
                }
            };

            var fpd = new FreezableStepData(dict, location);

            return new CompoundFreezableStep(ArrayStepFactory.Instance.TypeName, fpd, configuration);
        }

    }

    /// <summary>
    /// Contains helper methods for creating infix steps
    /// </summary>
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

            var data = new FreezableStepData(new StepParameterDict
            {
                {
                    new StepParameterReference(opData!.OperatorStepName), new FreezableStepProperty(opData.OperatorStep, errorLocation )
                },
                {
                    new StepParameterReference(opData.LeftName), left.Value
                },
                {
                    new StepParameterReference(opData.RightName), right.Value
                },

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
                    new EnumConstantFreezable(new Enumeration(nameof(MathOperator), mo.ToString()))
                ))
                .Concat(
                    Enum.GetValues<BooleanOperator>()
                .ToDictionary(mo => mo.GetDisplayName(), mo => new OperatorData(
                    nameof(ApplyBooleanOperator),
                    nameof(ApplyBooleanOperator.Left),
                    nameof(ApplyBooleanOperator.Right),
                    nameof(ApplyBooleanOperator.Operator),
                    new EnumConstantFreezable(new Enumeration(nameof(BooleanOperator), mo.ToString()))
                ))
                    )
                .Concat(
                    Enum.GetValues<CompareOperator>()
                .ToDictionary(mo => mo.GetDisplayName(), mo => new OperatorData(
                    CompareStepFactory.Instance.TypeName,
                    nameof(Compare<int>.Left),
                    nameof(Compare<int>.Right),
                    nameof(Compare<int>.Operator),
                    new EnumConstantFreezable(new Enumeration(nameof(CompareOperator), mo.ToString()))
                ))
                    )
                .Where(x => x.Key != MathOperator.None.ToString())

                .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);


    }
}
