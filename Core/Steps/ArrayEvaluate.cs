using System;
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
/// Read an array into memory so that it can be read more than once.
/// </summary>
/// <typeparam name="T"></typeparam>
[Alias("Evaluate")]
public class ArrayEvaluate<T> : CompoundStep<Array<T>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<T>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var array = await Array.Run(stateMonad, cancellationToken);

        if (array.IsFailure)
            return array.ConvertFailure<Array<T>>();

        var eager = await array.Value.Evaluate(cancellationToken).Map(x => x as Array<T>);

        return eager;
    }

    /// <summary>
    /// The array to evaluate
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<T>> Array { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayEvaluateStepFactory.Instance;

    private sealed class ArrayEvaluateStepFactory : ArrayStepFactory
    {
        private ArrayEvaluateStepFactory() { }
        public static GenericStepFactory Instance { get; } = new ArrayEvaluateStepFactory();

        /// <inheritdoc />
        public override Type StepType { get; } = typeof(ArrayEvaluate<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array of T";

        protected override string? LambdaPropertyName => null;

        /// <inheritdoc />
        protected override TypeReference GetOutputTypeReference(TypeReference memberTypeReference)
        {
            return new TypeReference.Array(memberTypeReference);
        }

        /// <inheritdoc />
        protected override Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
            CallerMetadata callerMetadata)
        {
            return callerMetadata.ExpectedType;
        }

        /// <inheritdoc />
        protected override string ArrayPropertyName => nameof(ArrayEvaluate<object>.Array);
    }
}

}
