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
/// Map each element of an array or entity stream to a new value.
/// This Step can be used to update elements of an array, or to update
/// entity property values.
/// The new value must have the same type as the original value.
/// </summary>
[SCLExample("ArrayMap [1, 2, 3, 4] Function: (<> + 1)", "[2, 3, 4, 5]")]
[SCLExample(
    @"Map Array: [
  ('type': 'A', 'value': 1)
  ('type': 'B', 'value': 2)
  ('type': 'A', 'value': 3)
] Using: (In <> Set: 'value' To: (<>['value'] + 1))",
    "[('type': \"A\" 'value': 2), ('type': \"B\" 'value': 3), ('type': \"A\" 'value': 4)]"
)]
[Alias("EntityMap")] // legacy name
[Alias("Map")]
public sealed class ArrayMap<T> : CompoundStep<Array<T>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<T>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var arrayResult = await Array.Run(stateMonad, cancellationToken);

        if (arrayResult.IsFailure)
            return arrayResult.ConvertFailure<Array<T>>();

        var currentState = stateMonad.GetState().ToImmutableDictionary();

        async ValueTask<T> Action(T record)
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

        var newStream = arrayResult.Value.SelectAwait(Action);

        return newStream;
    }

    /// <summary>
    /// The array or entity stream to map
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// A function to update the values and return the mapped entity
    /// </summary>
    [FunctionProperty(2)]
    [Required]
    [Alias("Using")]
    public LambdaFunction<T, T> Function { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayMapStepFactory.Instance;

    /// <summary>
    /// Filter entities according to a function.
    /// </summary>
    private sealed class ArrayMapStepFactory : ArrayStepFactory
    {
        private ArrayMapStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayMapStepFactory();

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
        protected override string ArrayPropertyName => nameof(ArrayMap<object>.Array);

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayMap<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array of T";

        protected override string LambdaPropertyName => nameof(ArrayMap<object>.Function);
    }
}

}
