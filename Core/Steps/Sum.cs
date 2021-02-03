using System;
using System.Collections.Generic;
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
/// Calculate the sum of a list of numbers
/// </summary>
[Alias("Add")]
public sealed class Sum : MathOperatorStep<Sum>
{
    /// <inheritdoc />
    protected override int Operate(IEnumerable<int> numbers)
    {
        return numbers.Sum();
    }
}

/// <summary>
/// Calculate the product of a list of numbers
/// </summary>
public sealed class Product : MathOperatorStep<Product>
{
    /// <inheritdoc />
    protected override int Operate(IEnumerable<int> numbers)
    {
        return numbers.Aggregate(1, (a, b) => a * b);
    }
}

/// <summary>
/// Subtract a list of numbers from a number
/// </summary>
public sealed class Subtract : MathOperatorStep<Subtract>
{
    /// <inheritdoc />
    protected override int Operate(IEnumerable<int> numbers)
    {
        var total = 0;
        var first = true;

        foreach (var number in numbers)
        {
            if (first)
            {
                total += number;
                first =  false;
            }
            else
            {
                total -= number;
            }
        }

        return total;
    }
}

/// <summary>
/// Divide a number by a list of numbers
/// </summary>
public sealed class Divide : MathOperatorStep<Divide>
{
    /// <inheritdoc />
    protected override int Operate(IEnumerable<int> numbers)
    {
        var total = 0;
        var first = true;

        foreach (var number in numbers)
        {
            if (first)
            {
                total += number;
                first =  false;
            }
            else
            {
                total /= number;
            }
        }

        return total;
    }
}

/// <summary>
/// Modulo a number by a list of numbers sequentially
/// </summary>
public sealed class Modulo : MathOperatorStep<Modulo>
{
    /// <inheritdoc />
    protected override int Operate(IEnumerable<int> numbers)
    {
        var total = 0;
        var first = true;

        foreach (var number in numbers)
        {
            if (first)
            {
                total += number;
                first =  false;
            }
            else
            {
                total %= number;
            }
        }

        return total;
    }
}

/// <summary>
/// Raises a number to the power of a list of numbers sequentially
/// </summary>
public sealed class Power : MathOperatorStep<Power>
{
    /// <inheritdoc />
    protected override int Operate(IEnumerable<int> numbers)
    {
        var total = 0;
        var first = true;

        foreach (var number in numbers)
        {
            if (first)
            {
                total += number;
                first =  false;
            }
            else
            {
                total = Convert.ToInt32(Math.Pow(total, number));
            }
        }

        return total;
    }
}

/// <summary>
/// Base class for all math operations
/// </summary>
public abstract class MathOperatorStep<TStep> : CompoundStep<int>
    where TStep : MathOperatorStep<TStep>, new()
{
    /// <summary>
    /// The numbers to operate on
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<int>> Numbers { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<TStep, int>();

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
