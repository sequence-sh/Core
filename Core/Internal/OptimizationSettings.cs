namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// Optimization settings while freezing
/// </summary>
public sealed class OptimizationSettings
{
    /// <summary>
    /// Create new Optimization Settings
    /// </summary>
    public OptimizationSettings(
        bool foldConstants,
        IReadOnlyDictionary<VariableName, InjectedVariable>? injectedVariables)
    {
        FoldConstants = foldConstants;

        InjectedVariables = injectedVariables
                         ?? ImmutableDictionary<VariableName, InjectedVariable>.Empty;
    }

    /// <summary>
    /// Whether to fold constants
    /// </summary>
    public bool FoldConstants { get; }

    /// <summary>
    /// Variables to inject
    /// </summary>
    public IReadOnlyDictionary<VariableName, InjectedVariable> InjectedVariables { get; }

    /// <summary>
    /// No optimization
    /// </summary>
    public static OptimizationSettings None { get; } = new(false, null);
}
