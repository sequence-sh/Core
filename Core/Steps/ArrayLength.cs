using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Counts the elements in an array or entities in an entity stream.
/// </summary>
[Alias("Length")]
[SCLExample("ArrayLength [1,2,3]", ExpectedOutput = "3")]
[SCLExample("Length Of: [1,2,3]",  ExpectedOutput = "3")]
public sealed class ArrayLength<T> : CompoundStep<int>
{
    /// <summary>
    /// The array to count.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("Of")]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<int, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return await Array.Run(stateMonad, cancellationToken)
            .Bind(x => x.CountAsync(cancellationToken));
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayLengthStepFactory.Instance;

    /// <summary>
    /// Counts the elements in an array.
    /// </summary>
    private sealed class ArrayLengthStepFactory : ArrayStepFactory
    {
        private ArrayLengthStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayLengthStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayLength<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Int32);

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            TypeReference.Actual.Integer;

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetadata)
        {
            var r = callerMetadata.CheckAllows(
                TypeReference.Actual.Integer,
                null
            );

            if (r.IsFailure)
                return r.ConvertFailure<TypeReference>();

            return new TypeReference.Array(TypeReference.Any.Instance);
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ArrayLength<object>.Array);

        protected override string? LambdaPropertyName => null;
    }
}
