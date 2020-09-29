using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Returns true if both operands are true
    /// </summary>
    public sealed class ApplyBooleanStepFactory : SimpleStepFactory<ApplyBooleanOperator, bool>
    {
        private ApplyBooleanStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static StepFactory Instance { get; } = new ApplyBooleanStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"[{nameof(ApplyBooleanOperator.Left)}] [{nameof(ApplyBooleanOperator.Operator)}] [{nameof(ApplyBooleanOperator.Right)}]");

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] {typeof(BooleanOperator)};

        /// <inheritdoc />
        public override IStepSerializer Serializer { get; } = new StepSerializer(
            new BooleanComponent(nameof(ApplyBooleanOperator.Left)),
            new SpaceComponent(),
            new EnumDisplayComponent<BooleanOperator>(nameof(ApplyBooleanOperator.Operator)),
            new SpaceComponent(),
            new BooleanComponent(nameof(ApplyBooleanOperator.Right))
        );
    }
}