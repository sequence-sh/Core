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
/// Returns true if all terms are true
/// </summary>
public sealed class And : BaseOperatorStep<And, bool, bool>
{
    /// <inheritdoc />
    protected override bool Operate(IEnumerable<bool> terms)
    {
        return terms.All(x => x);
    }
}

/// <summary>
/// Returns true if any terms are true
/// </summary>
public sealed class Or : BaseOperatorStep<Or, bool, bool>
{
    /// <inheritdoc />
    protected override bool Operate(IEnumerable<bool> terms)
    {
        return terms.Any(x => x);
    }
}

/// <summary>
/// Calculate the sum of a list of numbers
/// </summary>
[Alias("Add")]
public sealed class Sum : BaseOperatorStep<Sum, int, int>
{
    /// <inheritdoc />
    protected override int Operate(IEnumerable<int> terms)
    {
        return terms.Sum();
    }
}

/// <summary>
/// Calculate the product of a list of numbers
/// </summary>
public sealed class Product : BaseOperatorStep<Product, int, int>
{
    /// <inheritdoc />
    protected override int Operate(IEnumerable<int> terms)
    {
        return terms.Aggregate(1, (a, b) => a * b);
    }
}

/// <summary>
/// Subtract a list of numbers from a number
/// </summary>
public sealed class Subtract : BaseOperatorStep<Subtract, int, int>
{
    /// <inheritdoc />
    protected override int Operate(IEnumerable<int> terms)
    {
        var total = 0;
        var first = true;

        foreach (var number in terms)
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
public sealed class Divide : BaseOperatorStep<Divide, int, int>
{
    /// <inheritdoc />
    protected override int Operate(IEnumerable<int> terms)
    {
        var total = 0;
        var first = true;

        foreach (var number in terms)
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
public sealed class Modulo : BaseOperatorStep<Modulo, int, int>
{
    /// <inheritdoc />
    protected override int Operate(IEnumerable<int> terms)
    {
        var total = 0;
        var first = true;

        foreach (var number in terms)
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
public sealed class Power : BaseOperatorStep<Power, int, int>
{
    /// <inheritdoc />
    protected override int Operate(IEnumerable<int> terms)
    {
        var total = 0;
        var first = true;

        foreach (var number in terms)
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

        var r = Operate(termsResult.Value);

        return r;
    }

    /// <summary>
    /// Calculate the result
    /// </summary>
    protected abstract TOutput Operate(IEnumerable<TElement> terms);
}

}
