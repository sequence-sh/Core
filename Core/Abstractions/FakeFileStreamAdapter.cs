using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Thinktecture.IO;
using Thinktecture.IO.Adapters;
using Thinktecture.Win32.SafeHandles;

namespace Reductech.EDR.Core.Abstractions
{

/// <summary>
/// Implementation of IFileStream that uses an underlying stream
/// </summary>
public class FakeFileStreamAdapter : StreamAdapter, IFileStream
{
    /// <summary>
    /// The underlying stream
    /// </summary>
    public Stream Stream { get; }

    /// <inheritdoc />
    public FakeFileStreamAdapter(Stream stream) : base(stream) => Stream = stream;

    /// <inheritdoc />
    public FakeFileStreamAdapter(string s) : this(new MemoryStream(Encoding.ASCII.GetBytes(s))) { }

    /// <inheritdoc />
    public new FileStream UnsafeConvert()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void Flush(bool flushToDisk)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void Lock(long position, long length)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void Unlock(long position, long length)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool IsAsync => false;

    /// <inheritdoc />
    public string Name => "Fake File Stream";

    /// <inheritdoc />
    public ISafeFileHandle SafeFileHandle { get; } = null!;
}

}
