using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Asserts that two objects are equal.
/// Both objects must have the same type.
/// </summary>
public sealed class AssertEqual<T> : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var left = await Left.Run(stateMonad, cancellationToken);

        if (left.IsFailure)
            return left.ConvertFailure<Unit>();

        var right = await Right.Run(stateMonad, cancellationToken);

        if (right.IsFailure)
            return right.ConvertFailure<Unit>();

        var r = left.Value is not null && left.Value.Equals(right.Value);

        if (r)
            return Unit.Default;

        var lString = await SerializationMethods.GetStringAsync(left.Value);
        var rString = await SerializationMethods.GetStringAsync(right.Value);

        var error = ErrorCode.AssertionFailed
            .ToErrorBuilder($"Expected {lString} to equal {rString}")
            .WithLocation(this);

        return Result.Failure<Unit, IError>(error);
    }

    /// <summary>
    /// The first object to compare
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<T> Left { get; set; } = null!;

    /// <summary>
    /// The second object to compare
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<T> Right { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = AssertEqualStepFactory.Instance;

    private sealed class AssertEqualStepFactory : GenericStepFactory
    {
        private AssertEqualStepFactory() { }
        public static GenericStepFactory Instance { get; } = new AssertEqualStepFactory();

        /// <inheritdoc />
        public override Type StepType { get; } = typeof(AssertEqual<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation { get; } = nameof(Unit);

        /// <inheritdoc />
        protected override TypeReference GetOutputTypeReference(TypeReference memberTypeReference)
        {
            return TypeReference.Unit.Instance;
        }

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            TypeReference expectedTypeReference,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var left = freezableStepData
                .TryGetStep(nameof(Left), StepType)
                .Bind(x => x.TryGetOutputTypeReference(TypeReference.Any.Instance, typeResolver));

            if (left.IsFailure)
                return left.ConvertFailure<TypeReference>();

            var right = freezableStepData
                .TryGetStep(nameof(Right), StepType)
                .Bind(x => x.TryGetOutputTypeReference(TypeReference.Any.Instance, typeResolver));

            if (right.IsFailure)
                return right.ConvertFailure<TypeReference>();

            var r = TypeReference.Create(new[] { left.Value, right.Value });

            return r;
        }
    }
}

}
