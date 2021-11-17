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

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Filter an array according to a function.
/// </summary>
[Alias("FilterArray")]
[Alias("Filter")]
[SCLExample(
    "Filter <MyCsvFile> Predicate: (<>['column1'] == 'TypeA')",
    "[('column1': \"TypeA\" 'column2': 1)]",
    null,
    new[] { "MyCsvFile" },
    new[] { "[(column1: 'TypeA', column2: 1),(column1: 'TypeB', column2: 2)]" }
)]
public sealed class ArrayFilter<T> : CompoundStep<Array<T>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<T>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entityStreamResult = await Array.Run(stateMonad, cancellationToken);

        if (entityStreamResult.IsFailure)
            return entityStreamResult.ConvertFailure<Array<T>>();

        var currentState = stateMonad.GetState().ToImmutableDictionary();

        async IAsyncEnumerable<T> Filter(T record)
        {
            await using var scopedMonad = new ScopedStateMonad(
                stateMonad,
                currentState,
                Predicate.VariableNameOrItem,
                new KeyValuePair<VariableName, object>(Predicate.VariableNameOrItem, record!)
            );

            var result = await Predicate.StepTyped.Run(scopedMonad, cancellationToken);

            if (result.IsFailure)
                throw new ErrorException(result.Error);

            if (result.Value)
                yield return record;
        }

        var newStream = entityStreamResult.Value.SelectMany(Filter);

        return newStream;
    }

    /// <summary>
    /// The array to filter
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// A function that determines whether an entity should be included.
    /// </summary>
    [FunctionProperty(2)]
    [Required]
    public LambdaFunction<T, bool> Predicate { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayFilterStepFactory.Instance;

    /// <summary>
    /// Filter entities according to a function.
    /// </summary>
    private sealed class ArrayFilterStepFactory : ArrayStepFactory
    {
        private ArrayFilterStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayFilterStepFactory();

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
        protected override string ArrayPropertyName => nameof(ArrayFilter<object>.Array);

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayFilter<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array of T";

        protected override string LambdaPropertyName => nameof(ArrayFilter<object>.Predicate);
    }
}

}
