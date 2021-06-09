using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Gets the first index of an element in an array.
/// Returns -1 if the element is not present.
/// </summary>
public sealed class FindElement<T> : CompoundStep<int>
{
    /// <summary>
    /// The array to check.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// The element to look for.
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<T> Element { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<int, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var arrayResult = await Array.Run(stateMonad, cancellationToken);

        if (arrayResult.IsFailure)
            return arrayResult.ConvertFailure<int>();

        var elementResult = await Element.Run(stateMonad, cancellationToken);

        if (elementResult.IsFailure)
            return elementResult.ConvertFailure<int>();

        var indexResult =
            await arrayResult.Value.IndexOfAsync(elementResult.Value, cancellationToken);

        return indexResult;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => FindElementStepFactory.Instance;

    /// <summary>
    /// Gets the first index of an element in an array.
    /// </summary>
    public sealed class FindElementStepFactory : ArrayStepFactory
    {
        private FindElementStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new FindElementStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(FindElement<>);

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
            return callerMetadata
                .CheckAllows(TypeReference.Actual.Integer, null)
                .Map(_ => new TypeReference.Array(TypeReference.Any.Instance) as TypeReference);
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(FindElement<object>.Array);
    }
}

}
