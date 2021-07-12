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
/// Group array elements by the result of a function.
/// Entities in the resulting array will have two properties 'Key' and 'Values'
/// </summary>
public sealed class ArrayGroupBy<T> : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var arrayResult = await Array.Run(stateMonad, cancellationToken);

        if (arrayResult.IsFailure)
            return arrayResult.ConvertFailure<Array<Entity>>();

        var currentState = stateMonad.GetState().ToImmutableDictionary();

        async ValueTask<StringStream> Action(T record)
        {
            await using var scopedMonad = new ScopedStateMonad(
                stateMonad,
                currentState,
                Function.VariableNameOrItem,
                new KeyValuePair<VariableName, object>(Function.VariableNameOrItem, record!)
            );

            var result = await Function.StepTyped.Run(scopedMonad, cancellationToken);

            if (result.IsFailure)
                throw new ErrorException(result.Error);

            return result.Value;
        }

        var newStream = arrayResult.Value.GroupByAwait(Action);

        return newStream;
    }

    /// <summary>
    /// The array to map
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// A function to get the mapped entity
    /// </summary>
    [FunctionProperty(2)]
    [Required]
    public LambdaFunction<T, StringStream> Function { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayGroupByStepFactory.Instance;

    private sealed class ArrayGroupByStepFactory : ArrayStepFactory
    {
        private ArrayGroupByStepFactory() { }
        public static ArrayStepFactory Instance { get; } = new ArrayGroupByStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayGroupBy<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array of Entity";

        /// <inheritdoc />
        protected override TypeReference GetOutputTypeReference(TypeReference memberTypeReference)
        {
            return new TypeReference.Array(TypeReference.Actual.Entity);
        }

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetadata)
        {
            var r = callerMetadata.CheckAllows(
                new TypeReference.Array(TypeReference.Actual.Entity),
                null
            );

            if (r.IsFailure)
                return r.ConvertFailure<TypeReference>();

            return new TypeReference.Array(TypeReference.Any.Instance);
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ArrayGroupBy<object>.Array);

        protected override string LambdaPropertyName => nameof(ArrayGroupBy<object>.Function);
    }
}

}
