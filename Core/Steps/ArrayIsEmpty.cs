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
/// Checks if an array is empty.
/// </summary>
[Alias("IsArrayEmpty")]
[Alias("IsEmpty")]
public sealed class ArrayIsEmpty<T> : CompoundStep<bool>
{
    /// <summary>
    /// The array to check for emptiness.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<bool, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return await Array.Run(stateMonad, cancellationToken)
            .Bind(x => x.AnyAsync(cancellationToken))
            .Map(x => !x);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayIsEmptyStepFactory.Instance;
}

/// <summary>
/// Checks if an array is empty.
/// </summary>
public sealed class ArrayIsEmptyStepFactory : ArrayStepFactory
{
    private ArrayIsEmptyStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static GenericStepFactory Instance { get; } = new ArrayIsEmptyStepFactory();

    /// <inheritdoc />
    public override Type StepType => typeof(ArrayIsEmpty<>);

    /// <inheritdoc />
    public override string OutputTypeExplanation => nameof(Boolean);

    /// <inheritdoc />
    protected override TypeReference GetOutputTypeReference(TypeReference memberTypeReference) =>
        TypeReference.Actual.Bool;

    /// <inheritdoc />
    protected override string ArrayPropertyName => nameof(ArrayIsEmpty<object>.Array);

    /// <inheritdoc />
    protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
        TypeReference expectedTypeReference)
    {
        return expectedTypeReference
            .CheckAllows(TypeReference.Actual.Bool, StepType)
            .Map(_ => new TypeReference.Array(TypeReference.Any.Instance) as TypeReference);
    }
}

}
