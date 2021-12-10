namespace Reductech.EDR.Core.ExternalProcesses;

/// <summary>
/// Ignores all errors.
/// </summary>
public class IgnoreAllErrorHandler : IErrorHandler
{
    private IgnoreAllErrorHandler() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static IErrorHandler Instance = new IgnoreAllErrorHandler();

    /// <inheritdoc />
    public bool ShouldIgnoreError(string s) => true;
}
