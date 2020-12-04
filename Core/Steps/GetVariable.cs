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


namespace Reductech.EDR.Core.Steps
{

    /// <summary>
    /// Gets the value of a named variable.
    /// </summary>
    public sealed class GetVariable<T> : CompoundStep<T>
    {
        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<Result<T, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken) =>
            stateMonad.GetVariable<T>(Variable).MapError(x=>x.WithLocation(this));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        /// <inheritdoc />
        public override IStepFactory StepFactory => GetVariableStepFactory.Instance;

        /// <summary>
        /// The name of the variable to get.
        /// </summary>
        [VariableName]
        [Required]
        public VariableName Variable { get; set; }
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
        public override Type StepType => typeof(GetVariable<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.GetVariableName(nameof(GetVariable<object>.Variable), TypeName)

                .Map(x => new VariableTypeReference(x) as ITypeReference);


        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        public override IStepSerializer Serializer =>
            new StepSerializer(TypeName,
                new StepComponent(nameof(GetVariable<object>.Variable)));



        /// <summary>
        /// Create a freezable GetVariable step.
        /// </summary>
        public static IFreezableStep CreateFreezable(VariableName variableName, IErrorLocation location)
        {
            var dict = new Dictionary<string, FreezableStepProperty>
            {
                {nameof(GetVariable<object>.Variable), new FreezableStepProperty(variableName, location)}
            };

            var fpd = new FreezableStepData( dict, location);
            var step = new CompoundFreezableStep(Instance.TypeName, fpd, null);

            return step;
        }

    }
}