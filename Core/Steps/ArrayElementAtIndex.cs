using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Gets the array element at a particular index.
/// </summary>
[Alias("FromArray")]
public sealed class ArrayElementAtIndex<T> : CompoundStep<T>
{
    /// <summary>
    /// The array to check.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// The index to get the element at.
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("GetElement")]
    public IStep<int> Index { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(Array, Index, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<T>();

        var (array, index) = r.Value;

        var result = await array.ElementAtAsync(
            index,
            new ErrorLocation(this),
            cancellationToken
        );

        return result;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayElementAtIndexStepFactory.Instance;

    /// <inheritdoc />
    public override bool ShouldBracketWhenSerialized => false;

    /// <summary>
    /// Gets the array element at a particular index.
    /// </summary>
    private sealed class ArrayElementAtIndexStepFactory : ArrayStepFactory
    {
        private ArrayElementAtIndexStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayElementAtIndexStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayElementAtIndex<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        public override IStepSerializer Serializer => ArrayElementAtIndexSerializer.Instance;

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetadata)
        {
            return new TypeReference.Array(callerMetadata.ExpectedType);
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ArrayElementAtIndex<object>.Array);

        /// <inheritdoc />
        protected override string? LambdaPropertyName => null;
    }
}

}
