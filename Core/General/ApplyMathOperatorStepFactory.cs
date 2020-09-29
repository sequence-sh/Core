using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;

namespace Reductech.EDR.Core.General
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