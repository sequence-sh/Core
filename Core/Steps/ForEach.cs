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
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Do an action for each member of the list.
/// </summary>
[Alias("EntityForEach")]
public sealed class ForEach<T> : CompoundStep<Unit>
{
    /// <summary>
    /// The elements to iterate over.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// The action to perform repeatedly.
    /// </summary>
    [StepProperty(2)]
    [ScopedFunction]
    [Required]
    public IStep<Unit> Action { get; set; } = null!;

    /// <summary>
    /// The name of the variable to loop over.
    /// </summary>
    [VariableName(3)]
    [DefaultValueExplanation("<Entity>")]
    public VariableName Variable { get; set; } = VariableName.Entity;

    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var elements = await Array.Run(stateMonad, cancellationToken);

        if (elements.IsFailure)
            return elements.ConvertFailure<Unit>();

        var currentState = stateMonad.GetState().ToImmutableDictionary();

        async ValueTask<Result<Unit, IError>> Apply(T element, CancellationToken cancellation)
        {
            var scopedMonad = new ScopedStateMonad(
                stateMonad,
                currentState,
                new KeyValuePair<VariableName, object>(Variable, element!)
            );

            var result = await Action.Run(scopedMonad, cancellation);
            return result;
        }

        var finalResult = await elements.Value.ForEach(Apply, cancellationToken);

        return finalResult;
    }

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

    /// <inheritdoc />
    public override IStepFactory StepFactory => ForEachStepFactory.Instance;
}

/// <summary>
/// Do an action for each member of the list.
/// </summary>
public sealed class ForEachStepFactory : GenericStepFactory
{
    private ForEachStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static StepFactory Instance { get; } = new ForEachStepFactory();

    /// <inheritdoc />
    public override Type StepType => typeof(ForEach<>);

    /// <inheritdoc />
    protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) =>
        new ActualTypeReference(typeof(Unit));

    /// <inheritdoc />
    protected override Result<ITypeReference, IError> GetMemberType(
        FreezableStepData freezableStepData,
        TypeResolver typeResolver) => freezableStepData
        .TryGetStep(nameof(ForEach<object>.Array), StepType)
        .Bind(x => x.TryGetOutputTypeReference(typeResolver))
        .Bind(
            x => x.TryGetGenericTypeReference(typeResolver, 0)
                .MapError(e => e.WithLocation(freezableStepData))
        )
        .Map(x => x as ITypeReference);

    /// <inheritdoc />
    public override string OutputTypeExplanation => nameof(Unit);
}

}
