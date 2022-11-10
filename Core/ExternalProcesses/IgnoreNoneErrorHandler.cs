namespace Sequence.Core.ExternalProcesses;

/// <summary>
/// Does not ignore any errors.
/// </summary>
public class IgnoreNoneErrorHandler : IErrorHandler
{
    private IgnoreNoneErrorHandler() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static IErrorHandler Instance = new IgnoreNoneErrorHandler();

    /// <inheritdoc />
    public bool ShouldIgnoreError(string s) => false;
}
