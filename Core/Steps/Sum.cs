using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Calculate the sum of a list of numbers
/// </summary>
[Alias("Add")]
public sealed class Sum : MathOperatorStep
{
    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<Sum, int>();

    /// <inheritdoc />
    protected override int Operate(IEnumerable<int> numbers)
    {
        return numbers.Sum();
    }
}

/// <summary>
/// Base class for all math operations
/// </summary>
public abstract class MathOperatorStep : CompoundStep<int>
{
    /// <summary>
    /// The numbers to operate on
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<int>> Numbers { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<int, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var numbersResult =
            await Numbers.Run(stateMonad, cancellationToken).Bind(x => x.GetElements());

        if (numbersResult.IsFailure)
            return numbersResult.ConvertFailure<int>();

        var r = Operate(numbersResult.Value);

        return r;
    }

    /// <summary>
    /// Calculate the result
    /// </summary>
    protected abstract int Operate(IEnumerable<int> numbers);
}

}
