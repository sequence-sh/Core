using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Gets the last element of the array
/// </summary>
[Alias("Last")]
[SCLExample("ArrayLast [1,2,3]", ExpectedOutput = "3")]
public sealed class ArrayLast<T> : CompoundStep<T>
{
    /// <inheritdoc />
    protected override async Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var result = await Array
                .Run(stateMonad, cancellationToken)
                .Bind(x => x.GetElementsAsync(cancellationToken))
            ;

        if (result.IsFailure)
            return result.ConvertFailure<T>();

        if (result.Value.Any())
            return result.Value.Last();

        return new SingleError(new ErrorLocation(this), ErrorCode.IndexOutOfBounds);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => new SimpleStepFactory<ArrayLast<T>, T>();

    /// <summary>
    /// The array to get the lat element of
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;
}

}
