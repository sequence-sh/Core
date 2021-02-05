using Thinktecture;
using Thinktecture.IO;
using Thinktecture.IO.Adapters;

namespace Reductech.EDR.Core.Abstractions
{

/// <summary>
/// Default implementation of ICompression
/// </summary>
public class CompressionAdapter : ICompression
{
    /// <inheritdoc />
    public IStream Decompress(IStream stream)
    {
        var s = stream.ToImplementation()!;

        var result = new System.IO.Compression.GZipStream(
            s,
            System.IO.Compression.CompressionMode.Decompress
        );

        return new StreamAdapter(result);
    }

    /// <inheritdoc />
    public IStream Compress(IStream stream)
    {
        var s = stream.ToImplementation()!;

        var result = new System.IO.Compression.GZipStream(
            s,
            System.IO.Compression.CompressionMode.Compress
        );

        return new StreamAdapter(result);
    }

    /// <inheritdoc />
    public void ExtractToDirectory(
        string sourceArchiveFileName,
        string destinationDirectoryName,
        bool overwrite)
    {
        System.IO.Compression.ZipFile.ExtractToDirectory(
            sourceArchiveFileName,
            destinationDirectoryName,
            overwrite
        );
    }
}

}
