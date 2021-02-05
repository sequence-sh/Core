using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Thinktecture.IO;
using Thinktecture.IO.Adapters;
using Thinktecture.Win32.SafeHandles;

namespace Reductech.EDR.Core
{

public class FakeFileStreamAdapter : StreamAdapter, IFileStream
{
    [NotNull] public Stream Stream { get; }

    /// <inheritdoc />
    public FakeFileStreamAdapter([NotNull] Stream stream) : base(stream) => Stream = stream;

    /// <inheritdoc />
    public FakeFileStreamAdapter([NotNull] string s) : this(
        new MemoryStream(Encoding.ASCII.GetBytes(s))
    ) { }

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
    public bool IsAsync { get; }

    /// <inheritdoc />
    public string Name => "Fake File Stream";

    /// <inheritdoc />
    public ISafeFileHandle SafeFileHandle { get; }
}

}
