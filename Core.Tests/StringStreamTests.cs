using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Reductech.EDR.Core.Enums;
using Thinktecture.IO.Adapters;
using Xunit;
using Xunit.Sdk;

namespace Reductech.EDR.Core.Tests
{

public class StringStreamTests
{
    public const string StringToTest = "Hello World";

    [Fact]
    public void GetString_should_work_with_basic_String()
    {
        StringStream ss = StringToTest;

        ss.GetString().Should().Be(StringToTest);
    }

    [Fact]
    public void GetString_should_work_with_Stream()
    {
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(StringToTest));

        var ss = new StringStream(inputStream, EncodingEnum.UTF8);

        ss.GetString().Should().Be(StringToTest);
    }

    [Fact]
    public void GetString_should_work_with_Stream_Multiple_times()
    {
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(StringToTest));

        var ss = new StringStream(inputStream, EncodingEnum.UTF8);

        ss.GetString().Should().Be(StringToTest);
        ss.GetString().Should().Be(StringToTest);
        ss.GetString().Should().Be(StringToTest);
    }

    [Fact]
    public void GetStream_should_work_with_basic_String()
    {
        var ss = new StringStream(StringToTest);

        var (stream, encodingEnum) = ss.GetStream();

        var sr = new StreamReaderAdapter(stream, encodingEnum.Convert());

        sr.ReadToEnd().Should().Be(StringToTest);
    }

    [Fact]
    public void GetStream_should_work_with_Stream()
    {
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(StringToTest));

        var ss = new StringStream(inputStream, EncodingEnum.UTF8);

        var (stream, encodingEnum) = ss.GetStream();

        var sr = new StreamReaderAdapter(stream, encodingEnum.Convert());

        sr.ReadToEnd().Should().Be(StringToTest);
    }

    [Fact]
    public void GetString_should_close_original_Stream()
    {
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(StringToTest));

        var ss = new StringStream(inputStream, EncodingEnum.UTF8);

        ss.GetString().Should().Be(StringToTest);

        var alreadyDisposed = false;

        try
        {
            var _ = inputStream.Read(Span<byte>.Empty);
        }
        catch (ObjectDisposedException)
        {
            alreadyDisposed = true;
        }

        if (!alreadyDisposed)
            throw new XunitException("Stream had not yet been disposed");
    }
}

}
