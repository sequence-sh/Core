namespace Sequence.Core.Internal.Errors;

/// <summary>
/// Extension methods for ErrorBuilders that involve adding a location.
/// </summary>
public static class ErrorLocationHelper
{
    /// <summary>
    /// Add a ErrorLocation
    /// </summary>
    public static IError WithLocation(this IErrorBuilder errorBuilder, IStep step) =>
        errorBuilder.WithLocation(new ErrorLocation(step.Name, step.TextLocation));

    /// <summary>
    /// Add a FreezableErrorLocation
    /// </summary>
    public static IError WithLocation(this IErrorBuilder errorBuilder, IFreezableStep step) =>
        errorBuilder.WithLocation(new ErrorLocation(step.StepName, step.TextLocation));

    /// <summary>
    /// Add a FreezableErrorLocation
    /// </summary>
    public static IError WithLocation(this IErrorBuilder errorBuilder, FreezableStepData data) =>
        errorBuilder.WithLocation(data.Location);
}
