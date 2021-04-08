using System;
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
            TypeReference expectedTypeReference,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var variableName = freezableStepData
                .TryGetVariableName(nameof(GetVariable<object>.Variable), StepType);

            if (variableName.IsFailure)
                return variableName.ConvertFailure<TypeReference>();

            if (expectedTypeReference != TypeReference.Unknown.Instance
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

                return Result.Failure<TypeReference, IError>(
                    ErrorCode.WrongOutputType
                        .ToErrorBuilder(TypeName, tr.Name, expectedTypeReference.Name)
                        .WithLocation(freezableStepData)
                );
            }

            return new TypeReference.Variable(variableName.Value);
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
