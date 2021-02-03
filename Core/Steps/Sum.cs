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
/// Returns true if each term is greater than or equals to the next term
/// </summary>
public sealed class GreaterThanOrEqual<T> : CompareBaseOperatorStep<GreaterThanOrEqual<T>, T>
    where T : IComparable<T>
{
    /// <inheritdoc />
    protected override bool CheckComparisonValue(int v) => v >= 0;
}

/// <summary>
/// Returns true if each term is less than the next term
/// </summary>
public sealed class GreaterThan<T> : CompareBaseOperatorStep<GreaterThan<T>, T>
    where T : IComparable<T>
{
    /// <inheritdoc />
    protected override bool CheckComparisonValue(int v) => v > 0;
}

/// <summary>
/// Returns true if each term is less than or equals to the next term
/// </summary>
public sealed class LessThanOrEqual<T> : CompareBaseOperatorStep<LessThanOrEqual<T>, T>
    where T : IComparable<T>
{
    /// <inheritdoc />
    protected override bool CheckComparisonValue(int v) => v <= 0;
}

/// <summary>
/// Returns true if each term is less than the next term
/// </summary>
public sealed class LessThan<T> : CompareBaseOperatorStep<LessThan<T>, T>
    where T : IComparable<T>
{
    /// <inheritdoc />
    protected override bool CheckComparisonValue(int v) => v < 0;
}

/// <summary>
/// Returns true if each term is not equal to the next term
/// </summary>
public sealed class NotEquals<T> : CompareBaseOperatorStep<NotEquals<T>, T>
    where T : IComparable<T>
{
    /// <inheritdoc />
    protected override bool CheckComparisonValue(int v) => v != 0;
}

/// <summary>
/// Returns true is all terms are equal
/// </summary>
public sealed class Equals<T> : CompareBaseOperatorStep<Equals<T>, T>
    where T : IComparable<T>
{
    /// <inheritdoc />
    protected override bool CheckComparisonValue(int v) => v == 0;
}

/// <summary>
/// Base class for compare operations
/// </summary>
public abstract class
    CompareBaseOperatorStep<TStep, TElement> : GenericBaseOperatorStep<TStep, TElement,
        bool>
    where TStep : BaseOperatorStep<TStep, TElement, bool>, new()
    where TElement : IComparable<TElement>
{
    /// <summary>
    /// Check the result of comparing a term with the next term
    /// -1 means less than
    /// 0 means equals
    /// 1 means greater than
    /// </summary>
    protected abstract bool CheckComparisonValue(int v);

    /// <inheritdoc />
    protected override bool Operate(IEnumerable<TElement> terms)
    {
        var last = Maybe<TElement>.None;

        foreach (var term in terms)
        {
            if (last.HasValue)
            {
                var comparisonValue = last.Value.CompareTo(term);
                var checkResult     = CheckComparisonValue(comparisonValue);

                if (!checkResult)
                    return false;
            }

            last = term;
        }

        return true;
    }
}

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
/// Base class for generic operations
/// </summary>
public abstract class
    GenericBaseOperatorStep<TStep, TElement, TOutput> : BaseOperatorStep<TStep, TElement, TOutput>
    where TStep : BaseOperatorStep<TStep, TElement, TOutput>, new()
{
    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new GenericBaseOperatorStepFactory();

    /// <summary>
    /// Compares two items.
    /// </summary>
    public sealed class GenericBaseOperatorStepFactory : GenericStepFactory
    {
        /// <inheritdoc />
        public override Type StepType => typeof(TStep).GetGenericTypeDefinition();

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Boolean);

        /// <inheritdoc />
        protected override ITypeReference
            GetOutputTypeReference(ITypeReference memberTypeReference) =>
            new ActualTypeReference(typeof(bool));

        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var result = freezableStepData
                .TryGetStep(nameof(Terms), StepType)
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Bind(
                    x => x.TryGetGenericTypeReference(typeResolver, 0)
                        .MapError(e => e.WithLocation(freezableStepData))
                )
                .Map(x => x as ITypeReference);

            return result;
        }

        ///// <inheritdoc />
        //public override IStepSerializer Serializer => new StepSerializer(
        //    TypeName,
        //    new StepComponent(nameof(Compare<int>.Left)),
        //    SpaceComponent.Instance,
        //    new EnumDisplayComponent<CompareOperator>(nameof(Compare<int>.Operator)),
        //    SpaceComponent.Instance,
        //    new StepComponent(nameof(Compare<int>.Right))
        //);
    }
}

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

        var r = Operate(termsResult.Value);

        return r;
    }

    /// <summary>
    /// Calculate the result
    /// </summary>
    protected abstract TOutput Operate(IEnumerable<TElement> terms);
}

}
