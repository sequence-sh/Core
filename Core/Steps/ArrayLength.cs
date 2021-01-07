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
/// Counts the elements in an array.
/// </summary>
[Alias("Length")]
public sealed class ArrayLength<T> : CompoundStep<int>
{
    /// <summary>
    /// The array to count.
    /// </summary>
    [StepProperty(1)]
    [Required]
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
}

/// <summary>
/// Counts the elements in an array.
/// </summary>
public sealed class ArrayLengthStepFactory : GenericStepFactory
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
    protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) =>
        new ActualTypeReference(typeof(int));

    /// <inheritdoc />
    protected override Result<ITypeReference, IError> GetMemberType(
        FreezableStepData freezableStepData,
        TypeResolver typeResolver)
    {
        var r1 = freezableStepData.TryGetStep(nameof(ArrayLength<object>.Array), StepType)
            .Bind(x => x.TryGetOutputTypeReference(typeResolver))
            .Bind(
                x => x.TryGetGenericTypeReference(typeResolver, 0)
                    .MapError(e => e.WithLocation(freezableStepData))
            )
            .Map(x => x as ITypeReference);

        return r1;
    }
}

}
