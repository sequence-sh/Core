using System;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Base class for generic operations
/// </summary>
public abstract class
    GenericBaseOperatorStep<TStep, TElement, TOutput> : BaseOperatorStep<TStep, TElement, TOutput>
    where TStep : BaseOperatorStep<TStep, TElement, TOutput>, new()
{
    ///// <inheritdoc />
    //public override IStepFactory StepFactory { get; } = GenericBaseOperatorStepFactory.Instance;

    /// <summary>
    /// Compares two items.
    /// </summary>
    public sealed class GenericBaseOperatorStepFactory : GenericStepFactory
    {
        public GenericBaseOperatorStepFactory(SCLType elementType, SCLType outputType)
        {
            ElementType = elementType;
            OutputType  = outputType;
        }

        public SCLType ElementType { get; }
        public SCLType OutputType { get; }

        /// <inheritdoc />
        public override Type StepType => typeof(TStep).GetGenericTypeDefinition();

        /// <inheritdoc />
        public override string OutputTypeExplanation => OutputType.ToString();

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            new TypeReference.Actual(OutputType);

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetMemberType(
            TypeReference expectedTypeReference,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            expectedTypeReference.CheckAllows(new TypeReference.Actual(OutputType), StepType);

            var result = freezableStepData
                .TryGetStep(nameof(Terms), StepType)
                .Bind(
                    x => x.TryGetOutputTypeReference(
                        new TypeReference.Array(new TypeReference.Actual(ElementType)),
                        typeResolver
                    )
                )
                .Bind(
                    x => x.TryGetArrayMemberTypeReference(typeResolver)
                        .MapError(e => e.WithLocation(freezableStepData))
                );

            return result;
        }

        /// <inheritdoc />
        public override IStepSerializer Serializer { get; } = new ChainInfixSerializer(
            FormatTypeName(typeof(TStep)),
            new TStep().Operator
        );
    }
}

}
