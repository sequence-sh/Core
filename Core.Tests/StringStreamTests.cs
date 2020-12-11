using Reductech.EDR.Core.Parser;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Reductech.EDR.Core.Tests
{
    public class StringStreamTests
    {

        public const string StringToTest = "Hello World";

        [Fact]
        public void GetString_should_work_with_basic_String()
        {
            var ss = new StringStream(StringToTest);

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
        public void GetStream_should_work_with_basic_String()
        {
            var ss = new StringStream(StringToTest);

            var (stream, encodingEnum) = ss.GetStream();

            var sr = new StreamReader(stream, encodingEnum.Convert());

            sr.ReadToEnd().Should().Be(StringToTest);
        }

        [Fact]
        public void GetStream_should_work_with_Stream()
        {
            var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(StringToTest));

            var ss = new StringStream(inputStream, EncodingEnum.UTF8);

            var (stream, encodingEnum) = ss.GetStream();

            var sr = new StreamReader(stream, encodingEnum.Convert());

            sr.ReadToEnd().Should().Be(StringToTest);
        }


    }
}