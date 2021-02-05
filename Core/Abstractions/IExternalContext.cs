using Reductech.EDR.Core.ExternalProcesses;
using Thinktecture;

namespace Reductech.EDR.Core.Abstractions
{

/// <summary>
/// The external context of a sequence.
/// Includes the file system, external processes, and the console
/// </summary>
public interface IExternalContext
{
    /// <summary>
    /// For interacting with the file system
    /// </summary>
    public IFileSystem FileSystemHelper { get; }

    /// <summary>
    /// For interacting with external processes
    /// </summary>
    public IExternalProcessRunner ExternalProcessRunner { get; }

    /// <summary>
    /// For interacting with the console
    /// </summary>
    public IConsole Console { get; }
}

}
