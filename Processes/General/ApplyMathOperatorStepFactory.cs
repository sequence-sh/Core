using System;
using System.Collections.Generic;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Serialization;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Applies a mathematical operator to two integers.
    /// </summary>
    public class ApplyMathOperatorStepFactory : SimpleStepFactory<ApplyMathOperator, int>
    {
        private ApplyMathOperatorStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ApplyMathOperator, int> Instance { get; } = new ApplyMathOperatorStepFactory();

        ///// <inheritdoc />
        //protected override string ProcessNameTemplate => $"[{nameof(ApplyMathOperator.Left)}] [{nameof(ApplyMathOperator.Operator)}] [{nameof(ApplyMathOperator.Right)}]";

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] {typeof(MathOperator)};

        /// <inheritdoc />
        public override IStepSerializer Serializer { get; } = new StepSerializer(
            new IntegerComponent(nameof(ApplyMathOperator.Left)),
            new SpaceComponent(),
            new EnumDisplayComponent<MathOperator>(nameof(ApplyMathOperator.Operator)),
            new SpaceComponent(),
            new IntegerComponent(nameof(ApplyMathOperator.Right))
        );
    }
}