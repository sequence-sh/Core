namespace Reductech.Sequence.Core.Internal;

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
    /// Apply step level optimizations such as compiling regular expressions.
    /// By default, does nothing
    /// </summary>
    void ApplyOptimizations(
        StepFactoryStore sfs,
        IReadOnlyDictionary<VariableName, InjectedVariable> injectedVariables) { }
}

/// <summary>
/// A runnable step that is not a constant or enumerable.
/// </summary>
public interface ICompoundStep<T> : IStep<T>, ICompoundStep where T : ISCLObject { }
