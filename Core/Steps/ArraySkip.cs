﻿using System;
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
/// Skip the first n elements of the array
/// </summary>
[Alias("Skip")]
[SCLExample("ArraySkip [1, 2, 3] 2", ExpectedOutput = "[3]")]
public sealed class ArraySkip<T> : CompoundStep<Array<T>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<T>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(Array, Count, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<Array<T>>();

        var (array, count) = r.Value;

        return array.Skip(count);
    }

    /// <summary>
    /// The array to skip elements from
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// The number of elements to skip
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<int> Count { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = ArraySkipStepFactory.Instance;

    /// <summary>
    /// Counts the elements in an array.
    /// </summary>
    private sealed class ArraySkipStepFactory : ArrayStepFactory
    {
        private ArraySkipStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArraySkipStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArraySkip<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array of T";

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            new TypeReference.Array(memberTypeReference);

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetadata)
        {
            return callerMetadata.ExpectedType;
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ArraySkip<object>.Array);

        protected override string? LambdaPropertyName => null;
    }
}

}
