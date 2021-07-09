using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// A step that could be one of several options
/// </summary>
public sealed class OptionFreezableStep : IFreezableStep
{
    /// <summary>
    /// Create a new OptionFreezableStep
    /// </summary>
    public OptionFreezableStep(IReadOnlyList<IFreezableStep> options, TextLocation textLocation)
    {
        Options      = options;
        TextLocation = textLocation;
    }

    /// <summary>
    /// The options
    /// </summary>
    public IReadOnlyList<IFreezableStep> Options { get; }

    /// <inheritdoc />
    public TextLocation TextLocation { get; }

    /// <inheritdoc />
    public string StepName => string.Join(" or ", Options);

    /// <inheritdoc />
    public bool Equals(IFreezableStep? other) =>
        other is OptionFreezableStep ofs && Options.SequenceEqual(ofs.Options);

    /// <inheritdoc />
    public Result<IStep, IError> TryFreeze(CallerMetadata callerMetadata, TypeResolver typeResolver)
    {
        var optionErrors = new List<(IFreezableStep step, IError error)>();

        foreach (var freezableStep in Options)
            //Go through the options - use the first successful option or the first error
        {
            var r = freezableStep.TryFreeze(callerMetadata, typeResolver);

            if (r.IsSuccess)
                return r;

            optionErrors.Add((freezableStep, r.Error));
        }

        Debug.Assert(optionErrors.Any(), "OptionFreezableStep should have at least one option");

        //These all failed, but try to find one that was close to working
        foreach (var (step, error) in optionErrors)
        {
            var r = step.TryFreeze(
                callerMetadata with { ExpectedType = TypeReference.Any.Instance },
                typeResolver
            );

            if (
                r.IsSuccess) //This would have succeeded if the caller step expected a different type, so return the unexpected type error
                return Result.Failure<IStep, IError>(error);
        }

        return Result.Failure<IStep, IError>(optionErrors.First().error);
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<(VariableName variableName, TypeReference typeReference)>,
            IError>
        GetVariablesUsed(CallerMetadata callerMetadata, TypeResolver typeResolver)
    {
        IError? error = null;

        foreach (var freezableStep in Options)
        {
            var r = freezableStep.GetVariablesUsed(callerMetadata, typeResolver);

            if (r.IsSuccess)
            {
                var canAdd = true;

                foreach (var (variableName, typeReference) in r.Value)
                {
                    var canAddResult = typeResolver.CanAddType(variableName, typeReference);

                    if (canAddResult.IsFailure)
                    {
                        canAdd = false;
                        error  = canAddResult.Error.WithLocation(this);
                    }
                }

                if (canAdd)
                    return r;
            }

            else
                error = r.Error;
        }

        Debug.Assert(error != null, "OptionFreezableStep should have at least one option");

        return Result
            .Failure<IReadOnlyCollection<(VariableName variableName, TypeReference)>, IError
            >(error);
    }

    /// <inheritdoc />
    public Result<TypeReference, IError> TryGetOutputTypeReference(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver)
    {
        IError? error = null;

        foreach (var freezableStep in Options)
        {
            var r = freezableStep.TryGetOutputTypeReference(callerMetadata, typeResolver);

            if (r.IsSuccess)
            {
                var freezeResult = freezableStep.TryFreeze(callerMetadata, typeResolver);

                if (freezeResult.IsSuccess)
                    return r;

                error = freezeResult.Error;
            }

            else
                error = r.Error;
        }

        Debug.Assert(error != null, "OptionFreezableStep should have at least one option");

        return Result.Failure<TypeReference, IError>(error);
    }
}

}
