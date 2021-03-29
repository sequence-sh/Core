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
/// Reorder an array
/// </summary>
[Alias("SortArray")]
[Alias("Sort")]
public sealed class ArraySort<T> : CompoundStep<Array<T>>
{
    /// <summary>
    /// The array to sort.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// A function that gets the key to sort by from the variable &lt;Entity&gt;
    /// To sort by multiple properties, concatenate several keys
    /// </summary>
    [StepProperty(2)]
    [ScopedFunction]
    [DefaultValueExplanation("Default Ordering")]
    public IStep<StringStream>? KeySelector { get; set; } = null!;

    /// <summary>
    /// Whether to sort in descending order.
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("False")]
    public IStep<bool> Descending { get; set; } = new BoolConstant(false);

    /// <summary>
    /// The variable name to use in the keySelector if there is one.
    /// </summary>
    [VariableName(4)]
    [DefaultValueExplanation("<Entity>")]
    public VariableName Variable { get; set; } = VariableName.Entity;

    /// <inheritdoc />
    protected override async Task<Result<Array<T>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var array = await Array.Run(stateMonad, cancellationToken);

        if (array.IsFailure)
            return array.ConvertFailure<Array<T>>();

        var descending = await Descending.Run(stateMonad, cancellationToken);

        if (descending.IsFailure)
            return descending.ConvertFailure<Array<T>>();

        Array<T> sortedArray;

        if (KeySelector == null)
        {
            sortedArray = array.Value.Sort(descending.Value);
        }
        else
        {
            var currentState = stateMonad.GetState().ToImmutableDictionary();

            async ValueTask<string> GetKey(T entity, CancellationToken cancellation)
            {
                var scopedMonad = new ScopedStateMonad(
                    stateMonad,
                    currentState,
                    new KeyValuePair<VariableName, object>(VariableName.Entity, entity!)
                );

                var result = await KeySelector.Run(scopedMonad, cancellation)
                    .Map(x => x.GetStringAsync());

                if (result.IsFailure)
                    throw new ErrorException(result.Error);

                return result.Value;
            }

            sortedArray = array.Value.Sort(descending.Value, GetKey);
        }

        return sortedArray;
    }

    /// <inheritdoc />
    public override Result<TypeResolver, IError> TryGetScopedTypeResolver(
        TypeResolver baseContext,
        IFreezableStep scopedStep)
    {
        return baseContext.TryCloneWithScopedStep(
            Variable,
            TypeReference.Create(typeof(T)),
            TypeReference.Actual.String,
            scopedStep,
            new ErrorLocation(this)
        );
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArraySortStepFactory.Instance;

    /// <summary>
    /// Reorder an array.
    /// </summary>
    private sealed class ArraySortStepFactory : ArrayStepFactory
    {
        private ArraySortStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArraySortStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArraySort<>);

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            new TypeReference.Array(memberTypeReference);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array<T>";

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            TypeReference expectedTypeReference)
        {
            return expectedTypeReference;
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ArraySort<object>.Array);
    }
}

}
