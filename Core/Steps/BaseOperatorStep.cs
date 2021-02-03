using System.Collections.Generic;
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
/// Base class for operator operations
/// </summary>
public abstract class BaseOperatorStep<TStep, TElement, TOutput> : CompoundStep<TOutput>
    where TStep : BaseOperatorStep<TStep, TElement, TOutput>, new()
{
    /// <summary>
    /// The terms to operate on
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<TElement>> Terms { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<TStep, TOutput>();

    /// <inheritdoc />
    protected override async Task<Result<TOutput, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var termsResult =
            await Terms.Run(stateMonad, cancellationToken).Bind(x => x.GetElements());

        if (termsResult.IsFailure)
            return termsResult.ConvertFailure<TOutput>();

        var result = Operate(termsResult.Value);

        if (result.IsFailure)
            return result.MapError(x => x.WithLocation(this));

        return result.Value;
    }

    /// <summary>
    /// Calculate the result
    /// </summary>
    protected abstract Result<TOutput, IErrorBuilder> Operate(IEnumerable<TElement> terms);
}

}
