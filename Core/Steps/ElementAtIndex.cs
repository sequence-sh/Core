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
/// Gets the array element at a particular index.
/// </summary>
[Alias("FromArray")]
public sealed class ElementAtIndex<T> : CompoundStep<T>
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
        var arrayResult = await Array.Run(stateMonad, cancellationToken);

        if (arrayResult.IsFailure)
            return arrayResult.ConvertFailure<T>();

        var indexResult = await Index.Run(stateMonad, cancellationToken);

        if (indexResult.IsFailure)
            return indexResult.ConvertFailure<T>();

        var r = await arrayResult.Value.ElementAtAsync(
            indexResult.Value,
            new ErrorLocation(this),
            cancellationToken
        );

        return r;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => ElementAtIndexStepFactory.Instance;

    /// <inheritdoc />
    public override bool ShouldBracketWhenSerialized => false;

    /// <summary>
    /// Gets the array element at a particular index.
    /// </summary>
    private sealed class ElementAtIndexStepFactory : ArrayStepFactory
    {
        private ElementAtIndexStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ElementAtIndexStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ElementAtIndex<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        public override IStepSerializer Serializer => ElementAtIndexSerializer.Instance;

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
        protected override string ArrayPropertyName => nameof(ElementAtIndex<object>.Array);

        /// <inheritdoc />
        protected override string? LambdaPropertyName => null;
    }
}

}
