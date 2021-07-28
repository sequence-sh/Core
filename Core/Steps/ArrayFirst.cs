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
/// Gets the first element of an array
/// </summary>
/// <typeparam name="T"></typeparam>
[Alias("First")]
[SCLExample("ArrayFirst [1,2,3]", ExpectedOutput = "1")]
public sealed class ArrayFirst<T> : CompoundStep<T>
{
    /// <inheritdoc />
    protected override Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return Array.Run(stateMonad, cancellationToken)
            .Bind(x => x.ElementAtAsync(0, new ErrorLocation(this), cancellationToken));
    }

    /// <summary>
    /// The array to get the first element of
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => new SimpleStepFactory<ArrayFirst<T>, T>();
}

}
