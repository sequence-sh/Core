using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

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
    protected override Result<bool, IErrorBuilder> Operate(IEnumerable<TElement> terms)
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

}
