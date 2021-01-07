namespace Reductech.EDR.Core.ExternalProcesses
{

/// <summary>
/// Determines how to handle errors coming from an external step.
/// </summary>
public interface IErrorHandler
{
    /// <summary>
    /// Whether to ignore a particular error.
    /// </summary>
    public bool ShouldIgnoreError(string s);
}

}
