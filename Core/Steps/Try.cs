using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Logging;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Tries to execute a step and recovers if that step results in failure.
/// </summary>
[SCLExample(
    "Try (1 / 0) 0",
    "0",
    ExpectedLogs = new[] { "Error Caught in Divide: Attempt to Divide by Zero." }
)]
[SCLExample("Try (4 / 2)", "2")]
[SCLExample(
    "Try (1 / 0)",
    "0",
    Description  = "If the alternative is not set the default value is used.",
    ExpectedLogs = new[] { "Error Caught in Divide: Attempt to Divide by Zero." }
)]
[SCLExample(
    "Try (ArrayElementAtIndex [0,1,2,3] 4 ) 4",
    "4",
    ExpectedLogs = new[]
    {
        "Error Caught in ArrayElementAtIndex: Index was outside the bounds of the array."
    }
)]
public sealed class Try<T> : CompoundStep<T>
{
    /// <inheritdoc />
    protected override async Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var statementResult = await Statement.Run(stateMonad, cancellationToken);

        if (statementResult.IsSuccess)
            return statementResult.Value;

        LogSituation.StepErrorWasCaught.Log(
            stateMonad,
            this,
            Statement.Name,
            statementResult.Error.AsString
        );

        if (Alternative is not null)
            return await Alternative.Run(stateMonad, cancellationToken);

        return DefaultValues.GetDefault<T>();
    }

    /// <summary>
    /// The statement to try.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<T> Statement { get; set; } = null!;

    /// <summary>
    /// What to return if the statement returns an error
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("The Default Value")]
    public IStep<T>? Alternative { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => TryStepFactory.Instance;

    private sealed class TryStepFactory : GenericStepFactory
    {
        private TryStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new TryStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(Try<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var statementType = freezableStepData
                .TryGetStep(nameof(Statement), StepType)
                .Bind(
                    x => x.TryGetOutputTypeReference(
                        new CallerMetadata(TypeName, nameof(Statement), TypeReference.Any.Instance),
                        typeResolver
                    )
                );

            if (statementType.IsFailure)
                return statementType.ConvertFailure<TypeReference>();

            var alternativeStep = freezableStepData.TryGetStep(nameof(Alternative), StepType);

            if (alternativeStep.IsFailure)
                return statementType.Value;

            var alternativeType = alternativeStep.Value.TryGetOutputTypeReference(
                new CallerMetadata(TypeName, nameof(Alternative), TypeReference.Any.Instance),
                typeResolver
            );

            if (alternativeType.IsFailure)
                return alternativeType.ConvertFailure<TypeReference>();

            var r = TypeReference.Create(new[] { statementType.Value, alternativeType.Value });
            return r;
        }
    }
}

}
