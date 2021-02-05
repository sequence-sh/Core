using Reductech.EDR.Core.Abstractions;
using Thinktecture.IO;

namespace Reductech.EDR.Core.ExternalProcesses
{

/// <summary>
/// Abstraction layer for System.IO
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// The directory adapter
    /// </summary>
    IDirectory Directory { get; }

    /// <summary>
    /// The file adapter
    /// </summary>
    IFile File { get; }

    /// <summary>
    /// The compression adapter
    /// </summary>
    ICompression Compression { get; }
}

}
