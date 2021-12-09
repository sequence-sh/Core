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
/// Gets the first index of an element in an array.
/// The index starts at 0.
/// Returns -1 if the element is not present.
/// </summary>
[Alias("FindElement")]
[Alias("Find")]
[SCLExample("ArrayFind Array: [1, 2, 3] Element: 2", "1")]
[SCLExample("Find In: ['a', 'b', 'c'] Item: 'a'",    "0")]
[SCLExample("Find In: ['a', 'b', 'c'] Item: 'd'",    "-1")]
public sealed class ArrayFind<T> : CompoundStep<int>
{
    /// <summary>
    /// The array to check.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("In")]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// The element to look for.
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("Item")]
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
    public override IStepFactory StepFactory => ArrayFindStepFactory.Instance;

    /// <summary>
    /// Gets the first index of an element in an array.
    /// </summary>
    public sealed class ArrayFindStepFactory : ArrayStepFactory
    {
        private ArrayFindStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayFindStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayFind<>);

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
        protected override string ArrayPropertyName => nameof(ArrayFind<object>.Array);

        /// <inheritdoc />
        protected override string? LambdaPropertyName => null;
    }
}
