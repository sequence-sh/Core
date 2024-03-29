﻿namespace Sequence.Core.Steps;

/// <summary>
/// Gets the last element of an array or entity stream
/// </summary>
[Alias("Last")]
[Alias("GetLastItem")]
[SCLExample("ArrayLast [1,2,3]",                        ExpectedOutput = "3")]
[SCLExample("ArrayLast ['a', 'b', 'c']",                ExpectedOutput = "c")]
[SCLExample("ArrayLast [('a': 1), ('a': 2), ('a': 3)]", ExpectedOutput = "('a': 3)")]
[SCLExample("GetLastItem In: [1,2,3]",                  ExpectedOutput = "3")]
[AllowConstantFolding]
public sealed class ArrayLast<T> : CompoundStep<T> where T : ISCLObject
{
    /// <inheritdoc />
    protected override async ValueTask<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var result = await Array
                .Run(stateMonad, cancellationToken)
                .Bind(x => x.GetElementsAsync(cancellationToken))
            ;

        if (result.IsFailure)
            return result.ConvertFailure<T>();

        if (result.Value.Any())
            return result.Value.Last();

        return new SingleError(new ErrorLocation(this), ErrorCode.IndexOutOfBounds);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayLastStepFactory.Instance;

    /// <summary>
    /// The array to get the last element of
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("In")]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <summary>
    /// Gets the array element at a particular index.
    /// </summary>
    private sealed class ArrayLastStepFactory : ArrayStepFactory
    {
        private ArrayLastStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayLastStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayLast<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        public override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetadata)
        {
            return new TypeReference.Array(callerMetadata.ExpectedType);
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ArrayLast<ISCLObject>.Array);

        /// <inheritdoc />
        protected override string? LambdaPropertyName => null;
    }
}
