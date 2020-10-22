using System;
using System.Collections.Generic;
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
    public sealed class GetVariable<T> : CompoundStep<T>
    {
        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<Result<T, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken) =>
            stateMonad.GetVariable<T>(VariableName).MapError(x=>x.WithLocation(this));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        /// <inheritdoc />
        public override IStepFactory StepFactory => GetVariableStepFactory.Instance;

        /// <summary>
        /// The name of the variable to get.
        /// </summary>
        [VariableName] [Required]
        public VariableName VariableName { get; set; }
    }

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
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.GetVariableName(nameof(GetVariable<object>.VariableName), TypeName)
                .MapError(x=> x.WithLocation(new FreezableStepErrorLocation(this, freezableStepData)))

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
            var dict = new Dictionary<string, VariableName>
            {
                {nameof(GetVariable<object>.VariableName), variableName}
            };

            var fpd = new FreezableStepData(null, dict, null);
            var step = new CompoundFreezableStep(Instance, fpd, null);

            return step;
        }

    }
}