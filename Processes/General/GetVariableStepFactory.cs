using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Serialization;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Gets the value of a named variable.
    /// </summary>
    public class GetVariableStepFactory : GenericStepFactory
    {
        private GetVariableStepFactory() { }

        /// <summary>
        /// The Instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new GetVariableStepFactory();


        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"[{nameof(GetVariable<object>.VariableName)}]");

        /// <inheritdoc />
        public override Type StepType => typeof(GetVariable<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableStepData freezableStepData) =>
            freezableStepData.GetVariableName(nameof(GetVariable<object>.VariableName))
                .Map(x => new VariableTypeReference(x) as ITypeReference);


        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        public override IStepSerializer Serializer { get; } = new StepSerializer(
            new VariableNameComponent(nameof(GetVariable<object>.VariableName)));



        /// <summary>
        /// Create a freezable GetVariable step.
        /// </summary>
        public static IFreezableStep CreateFreezable(VariableName variableName)
        {
            var dict = new Dictionary<string, StepMember>
            {
                {nameof(GetVariable<object>.VariableName), new StepMember(variableName)}
            };

            var fpd = new FreezableStepData(dict);

            return new CompoundFreezableStep(Instance, fpd, null);
        }

    }
}