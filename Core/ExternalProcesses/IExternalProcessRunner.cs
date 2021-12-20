using System.Text;

namespace Reductech.Sequence.Core.ExternalProcesses;

/// <summary>
/// Runs external processes.
/// </summary>
public interface IExternalProcessRunner
{
    /// <summary>
    /// Runs an external step and returns the output and errors
    /// </summary>
    /// <param name="processPath">The path to the step</param>
    /// <param name="errorHandler">The error handler.</param>
    /// <param name="arguments">The arguments to provide to the step. These will all be escaped</param>
    /// <param name="environmentVariables">Environment Variables to pass to the process</param>
    /// <param name="encoding">The encoding to use for process output streams</param>
    /// <param name="stateMonad"></param>
    /// <param name="callingStep"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The output of the step</returns>
    Task<Result<Unit, IErrorBuilder>> RunExternalProcess(
        string processPath,
        IErrorHandler errorHandler,
        IEnumerable<string> arguments,
        IReadOnlyDictionary<string, string> environmentVariables,
        Encoding encoding,
        IStateMonad stateMonad,
        IStep? callingStep,
        CancellationToken cancellationToken);

    /// <summary>
    /// Starts an external process.
    /// </summary>
    Result<IExternalProcessReference, IErrorBuilder> StartExternalProcess(
        string processPath,
        IEnumerable<string> arguments,
        IReadOnlyDictionary<string, string> environmentVariables,
        Encoding encoding,
        IStateMonad stateMonad,
        IStep? callingStep);
}
