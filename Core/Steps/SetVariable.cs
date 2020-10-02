using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

    /// <summary>
    /// Gets the value of a named variable.
    /// </summary>
    public sealed class SetVariable<T> : CompoundStep<Unit>
    {

        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad) =>
            Value.Run(stateMonad)
                .Bind(x => stateMonad.SetVariable(VariableName, x));

        /// <inheritdoc />
        public override IStepFactory StepFactory => SetVariableStepFactory.Instance;

        /// <summary>
        /// The name of the variable to set.
        /// </summary>
        [VariableName]
        [Required]
        public VariableName VariableName { get; set; }

        /// <summary>
        /// The value to set the variable to.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> Value { get; set; } = null!;
    }

    /// <summary>
    /// Sets the value of a named variable.
    /// </summary>
    public class SetVariableStepFactory : StepFactory
    {
        private SetVariableStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static StepFactory Instance { get; } = new SetVariableStepFactory();

        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(FreezableStepData freezableStepData,
            TypeResolver typeResolver) => new ActualTypeReference(typeof(Unit));

        /// <inheritdoc />
        public override Type StepType => typeof(SetVariable<>);


        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"[{nameof(SetVariable<object>.VariableName)}] = [{nameof(SetVariable<object>.Value)}]");

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableArray<Type>.Empty;

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Unit);


        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName,
            FreezableStepData freezableStepData, TypeResolver typeResolver)
        {
            var result = freezableStepData.GetArgument(nameof(SetVariable<object>.Value))
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Map(Maybe<ITypeReference>.From);

            return result;
        }

        /// <inheritdoc />
        protected override Result<ICompoundStep> TryCreateInstance(StepContext stepContext, FreezableStepData freezableStepData) =>
            freezableStepData.GetVariableName(nameof(SetVariable<object>.VariableName))
                .Bind(x => stepContext.TryGetTypeFromReference(new VariableTypeReference(x)))
                .Bind(x => TryCreateGeneric(typeof(SetVariable<>), x));


        /// <inheritdoc />
        public override IStepSerializer Serializer { get; } = new StepSerializer(
            new VariableNameComponent(nameof(SetVariable<object>.VariableName)),
            new SpaceComponent(),
            new FixedStringComponent("="),
            new SpaceComponent(),
            new AnyPrimitiveComponent(nameof(SetVariable<object>.Value))
        );


        /// <summary>
        /// Create a freezable SetVariable step.
        /// </summary>
        public static IFreezableStep CreateFreezable(VariableName variableName, IFreezableStep value)
        {
            var dict = new Dictionary<string, StepMember>
            {
                {nameof(SetVariable<object>.VariableName), new StepMember(variableName)},
                {nameof(SetVariable<object>.Value), new StepMember(value)}
            };

            var fpd = new FreezableStepData(dict);

            return new CompoundFreezableStep(Instance, fpd, null);
        }
    }
}