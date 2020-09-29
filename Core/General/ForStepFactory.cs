using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Do an action for each value of a given variable in a range.
    /// </summary>
    public class ForStepFactory : SimpleStepFactory<For, Unit>
    {
        private ForStepFactory() { }


        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<For, Unit> Instance { get; } = new ForStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"For [{nameof(For.VariableName)}] = [{nameof(For.From)}]; [{nameof(For.VariableName)}] <= [{nameof(For.To)}]; += [{nameof(For.Increment)}]; [{nameof(For.Action)}]");


        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName, FreezableStepData freezableStepData) => Maybe<ITypeReference>.From(new ActualTypeReference(typeof(int)));
    }
}