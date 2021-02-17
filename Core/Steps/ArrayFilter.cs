using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
/// Filter an array according to a function.
/// </summary>
[Alias("FilterArray")]
[Alias("Filter")]
public sealed class ArrayFilter<T> : CompoundStep<Array<T>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<T>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entityStreamResult = await Array.Run(stateMonad, cancellationToken);

        if (entityStreamResult.IsFailure)
            return entityStreamResult.ConvertFailure<Array<T>>();

        var currentState = stateMonad.GetState().ToImmutableDictionary();

        async IAsyncEnumerable<T> Filter(T record)
        {
            using var scopedMonad = new ScopedStateMonad(
                stateMonad,
                currentState,
                new KeyValuePair<VariableName, object>(Variable, record!)
            );

            var result = await Predicate.Run(scopedMonad, cancellationToken);

            if (result.IsFailure)
                throw new ErrorException(result.Error);

            if (result.Value)
                yield return record;
        }

        var newStream = entityStreamResult.Value.SelectMany(Filter);

        return newStream;
    }

    /// <summary>
    /// The array to filter
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// A function that determines whether an entity should be included.
    /// </summary>
    [StepProperty(2)]
    [ScopedFunction]
    [Required]
    public IStep<bool> Predicate { get; set; } = null!;

    /// <summary>
    /// The variable name to use in the predicate.
    /// </summary>
    [VariableName(3)]
    [DefaultValueExplanation("<Entity>")]
    public VariableName Variable { get; set; } = VariableName.Entity;

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayFilterStepFactory.Instance;

    /// <inheritdoc />
    public override Result<TypeResolver, IError> TryGetScopedTypeResolver(
        TypeResolver baseContext,
        IFreezableStep scopedStep)
    {
        return baseContext.TryCloneWithScopedStep(
            Variable,
            new ActualTypeReference(typeof(T)),
            scopedStep,
            new ErrorLocation(this)
        );
    }
}

/// <summary>
/// Filter entities according to a function.
/// </summary>
public sealed class ArrayFilterStepFactory : GenericStepFactory
{
    private ArrayFilterStepFactory() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static GenericStepFactory Instance { get; } = new ArrayFilterStepFactory();

    /// <inheritdoc />
    protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) =>
        new GenericTypeReference(typeof(Array<>), new[] { memberTypeReference });

    /// <inheritdoc />
    protected override Result<ITypeReference, IError> GetMemberType(
        FreezableStepData freezableStepData,
        TypeResolver typeResolver) => freezableStepData
        .TryGetStep(nameof(ArrayFilter<object>.Array), StepType)
        .Bind(x => x.TryGetOutputTypeReference(typeResolver))
        .Bind(
            x => x.TryGetGenericTypeReference(typeResolver, 0)
                .MapError(e => e.WithLocation(freezableStepData))
        )
        .Map(x => x as ITypeReference);

    /// <inheritdoc />
    public override Type StepType => typeof(ArrayFilter<>);

    /// <inheritdoc />
    public override string OutputTypeExplanation => "Array<T>";
}

}
