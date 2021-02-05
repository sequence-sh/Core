using Thinktecture.IO;

namespace Reductech.EDR.Core.Abstractions
{

/// <summary>
/// Abstraction layer for System.IO.Compression
/// </summary>
public interface ICompression
{
    /// <summary>
    /// Decompress the contents of a stream
    /// </summary>
    public IStream Decompress(IStream stream);

    /// <summary>
    /// Compress the contents of a stream
    /// </summary>
    public IStream Compress(IStream stream);

    /// <summary>
    /// Extract a file to a directory
    /// </summary>
    public void ExtractToDirectory(
        string sourceArchiveFileName,
        string destinationDirectoryName,
        bool overwrite);
}

}
