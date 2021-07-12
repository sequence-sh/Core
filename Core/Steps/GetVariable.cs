using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Gets the value of a named variable.
/// </summary>
public sealed class GetVariable<T> : CompoundStep<T>
{
    /// <inheritdoc />
    #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    protected override async Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken) =>
        stateMonad.GetVariable<T>(Variable).MapError(x => x.WithLocation(this));
    #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

    /// <inheritdoc />
    public override IStepFactory StepFactory => GetVariableStepFactory.Instance;

    /// <summary>
    /// The name of the variable to get.
    /// </summary>
    [VariableName(1)]
    [Required]
    public VariableName Variable { get; set; }

    /// <inheritdoc />
    public override string Name => Variable == default ? base.Name : $"Get {Variable.Serialize()}";

    /// <summary>
    /// Gets the value of a named variable.
    /// </summary>
    private class GetVariableStepFactory : GenericStepFactory
    {
        private GetVariableStepFactory() { }

        /// <summary>
        /// The Instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new GetVariableStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(GetVariable<>);

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var variableName = freezableStepData
                .TryGetVariableName(nameof(GetVariable<object>.Variable), StepType);

            if (variableName.IsFailure)
                return variableName.ConvertFailure<TypeReference>();

            var expectedTypeReference = callerMetadata.ExpectedType;

            if (!expectedTypeReference.IsUnknown
             && typeResolver.Dictionary.TryGetValue(variableName.Value, out var tr))
            {
                if (tr.Allow(expectedTypeReference, typeResolver))
                {
                    return expectedTypeReference;
                }
                else if (expectedTypeReference.Allow(tr, typeResolver))
                {
                    return tr;
                }

                return callerMetadata.GetWrongTypeError(
                    variableName.Value.Serialize(),
                    tr.Name,
                    new ErrorLocation(
                        TypeName,
                        freezableStepData.Location
                    )
                );
            }

            return new TypeReference.Variable(variableName.Value);
        }

        /// <inheritdoc />
        public override IEnumerable<UsedVariable> GetVariablesUsed(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var vn = freezableStepData.TryGetVariableName(
                nameof(GetVariable<object>.Variable),
                StepType
            );

            if (vn.IsFailure)
                yield break;

            yield return new(vn.Value, callerMetadata.ExpectedType, freezableStepData.Location);
        }

        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        public override IStepSerializer Serializer => new StepSerializer(
            TypeName,
            new StepComponent(nameof(GetVariable<object>.Variable))
        );
    }
}

}
