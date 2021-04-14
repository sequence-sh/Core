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
/// Runs another step, reads the output to the end, and ignores it.
/// </summary>
[Alias("Run")]
public sealed class RunStep<T> : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var output = await Step.Run(stateMonad, cancellationToken);

        if (output.IsFailure)
            return output.ConvertFailure<Unit>();

        var r = await ReadToEnd(output.Value, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<Unit>();

        return Unit.Default;
    }

    /// <summary>
    /// Reads all data in the object, returning any errors.
    /// </summary>
    public static async Task<Result<Unit, IError>> ReadToEnd(
        object? o,
        CancellationToken cancellation)
    {
        switch (o)
        {
            case IArray array:
            {
                var elements = await array.GetObjectsAsync(cancellation);

                if (elements.IsFailure)
                    return elements.ConvertFailure<Unit>();

                foreach (var element in elements.Value)
                {
                    var r = await ReadToEnd(element, cancellation);

                    if (r.IsFailure)
                        return r;
                }

                return Unit.Default;
            }
            case Entity entity:
            {
                foreach (var property in entity)
                {
                    var r = await ReadToEnd(property.BaseValue, cancellation);

                    if (r.IsFailure)
                        return r;
                }

                return Unit.Default;
            }
            case StringStream stringStream:
                await stringStream.GetStringAsync();
                return Unit.Default;
            default: return Unit.Default;
        }
    }

    /// <summary>
    /// The step to run.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<T> Step { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = RunStepFactory.Instance;

    private sealed class RunStepFactory : GenericStepFactory
    {
        private RunStepFactory() { }
        public static GenericStepFactory Instance { get; } = new RunStepFactory();

        /// <inheritdoc />
        public override Type StepType { get; } = typeof(RunStep<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Unit);

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            TypeReference.Unit.Instance;

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            TypeReference expectedTypeReference,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var checkResult =
                expectedTypeReference.CheckAllows(
                    TypeReference.Unit.Instance,
                    StepType,
                    typeResolver
                );

            if (checkResult.IsFailure)
                return checkResult.ConvertFailure<TypeReference>()
                    .MapError(x => x.WithLocation(freezableStepData));

            var stepTypeReference = freezableStepData.TryGetStep(nameof(Step), StepType)
                .Bind(x => x.TryGetOutputTypeReference(TypeReference.Any.Instance, typeResolver));

            return stepTypeReference;
        }
    }
}

}
