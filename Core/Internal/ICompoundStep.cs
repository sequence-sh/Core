using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// A runnable step that is not a constant.
/// </summary>
public interface ICompoundStep : IStep
{
    /// <summary>
    /// The factory used to create steps of this type.
    /// </summary>
    IStepFactory StepFactory { get; }

    /// <summary>
    /// Tries to get the scoped context for this step.
    /// Returns an error if this step does not have any scoped functions.
    /// </summary>
    Result<TypeResolver, IError> TryGetScopedTypeResolver(
        TypeResolver baseTypeResolver,
        IFreezableStep scopedStep);
}

/// <summary>
/// A runnable step that is not a constant or enumerable.
/// </summary>
public interface ICompoundStep<T> : IStep<T>, ICompoundStep { }

}
