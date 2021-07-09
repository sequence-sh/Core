using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Gets the value of the automatic variable
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class GetAutomaticVariable<T> : CompoundStep<T>
{
    /// <inheritdoc />
    #pragma warning disable 1998
    protected override async Task<Result<T, IError>> Run(
        #pragma warning restore 1998
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        if (stateMonad.AutomaticVariable.HasNoValue)
            return Result.Failure<T, IError>(
                ErrorCode.AutomaticVariableNotSet.ToErrorBuilder().WithLocation(this)
            );

        var result = stateMonad.GetVariable<T>(stateMonad.AutomaticVariable.Value)
            .MapError(x => x.WithLocation(this));

        return result;
    }

    /// <inheritdoc />
    public override bool ShouldBracketWhenSerialized => false;

    /// <inheritdoc />
    public override IStepFactory StepFactory => GetAutomaticVariableStepFactory.Instance;

    private class GetAutomaticVariableStepFactory : GenericStepFactory
    {
        private GetAutomaticVariableStepFactory() { }
        public static GenericStepFactory Instance { get; } = new GetAutomaticVariableStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(GetAutomaticVariable<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        public override IEnumerable<(VariableName variableName, TypeReference type)>
            GetVariablesSet(
                CallerMetadata callerMetadata,
                FreezableStepData freezableStepData,
                TypeResolver typeResolver)
        {
            yield return (VariableName.Item, callerMetadata.ExpectedType);
        }

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var avr = typeResolver.AutomaticVariableName;

            if (avr.HasNoValue)
                return ErrorCode.AutomaticVariableNotSet.ToErrorBuilder()
                    .WithLocationSingle(
                        new ErrorLocation(
                            TypeName,
                            freezableStepData.Location
                        )
                    );

            var expectedTypeReference = callerMetadata.ExpectedType;

            if (!expectedTypeReference.IsUnknown
             && typeResolver.Dictionary.TryGetValue(avr.Value, out var tr))
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
                    avr.Value.Serialize(),
                    tr.Name,
                    new ErrorLocation(
                        TypeName,
                        freezableStepData.Location
                    )
                );
            }

            return TypeReference.AutomaticVariable.Instance;
        }

        /// <inheritdoc />
        public override IStepSerializer Serializer =>
            GetAutomaticVariableStepSerializer.SerializerInstance;

        private class GetAutomaticVariableStepSerializer : IStepSerializer
        {
            private GetAutomaticVariableStepSerializer() { }

            /// <summary>
            /// The instance
            /// </summary>
            public static IStepSerializer SerializerInstance { get; } =
                new GetAutomaticVariableStepSerializer();

            /// <inheritdoc />
            public string Serialize(IEnumerable<StepProperty> stepProperties)
            {
                return "<>";
            }
        }
    }
}

}
