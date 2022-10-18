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
        bool stepOptimizations,
        IReadOnlyDictionary<VariableName, InjectedVariable>? injectedVariables)
    {
        FoldConstants     = foldConstants;
        StepOptimizations = stepOptimizations;

        InjectedVariables = injectedVariables
                         ?? ImmutableDictionary<VariableName, InjectedVariable>.Empty;
    }

    /// <summary>
    /// Whether to fold constants
    /// </summary>
    public bool FoldConstants { get; }

    /// <summary>
    /// Enables step level optimizations - e.g. compiling regular expressions
    /// </summary>
    public bool StepOptimizations { get; }

    /// <summary>
    /// Variables to inject
    /// </summary>
    public IReadOnlyDictionary<VariableName, InjectedVariable> InjectedVariables { get; }

    /// <summary>
    /// No optimization
    /// </summary>
    public static OptimizationSettings None { get; } = new(false, false, null);
}
