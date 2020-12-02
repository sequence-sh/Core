using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
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
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            return await Value.Run(stateMonad, cancellationToken)
                .Bind(x => stateMonad.SetVariable(Variable, x));
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => SetVariableStepFactory.Instance;

        /// <summary>
        /// The name of the variable to set.
        /// </summary>
        [VariableName]
        [Required]
        public VariableName Variable { get; set; }

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
        public override Result<ITypeReference, IError> TryGetOutputTypeReference(FreezableStepData freezableStepData,
            TypeResolver typeResolver) => new ActualTypeReference(typeof(Unit));

        /// <inheritdoc />
        public override Type StepType => typeof(SetVariable<>);


        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"[{nameof(SetVariable<object>.Variable)}] = [{nameof(SetVariable<object>.Value)}]");

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableArray<Type>.Empty;

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Unit);


        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>, IError> GetTypeReferencesSet(VariableName variableName,
            FreezableStepData freezableStepData, TypeResolver typeResolver, StepFactoryStore stepFactoryStore)
        {
            var result = freezableStepData.GetStep(nameof(SetVariable<object>.Value), TypeName)

                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Map(Maybe<ITypeReference>.From);

            return result;
        }

        /// <inheritdoc />
        protected override Result<ICompoundStep, IError> TryCreateInstance(StepContext stepContext,
            FreezableStepData freezeData) =>
            freezeData.GetVariableName(nameof(SetVariable<object>.Variable), TypeName)
                .Bind(x => stepContext.TryGetTypeFromReference(new VariableTypeReference(x)).MapError(e=> e.WithLocation(this, freezeData)))
                .Bind(x => TryCreateGeneric(typeof(SetVariable<>), x).MapError(e=> e.WithLocation(this, freezeData)));





        /// <inheritdoc />
        public override IStepSerializer Serializer  =>
            new StepSerializer(TypeName, new StepComponent(nameof(SetVariable<object>.Variable)),
            SpaceComponent.Instance,
            new FixedStringComponent("="),
            SpaceComponent.Instance,
            new StepComponent(nameof(SetVariable<object>.Value))
        );


    }
}