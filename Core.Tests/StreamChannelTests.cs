using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Xunit;

namespace Reductech.EDR.Core.Tests
{
    public class StreamChannelTests
    {
        public enum StreamReaderSource
        {
            StreamReaderOne,
            StreamReaderTwo
        }

        [Fact]
        public async Task TestReadingEmptyStream()
        {
            var mock = new Mock<TextReader>(MockBehavior.Strict);
            mock.Setup(m => m.ReadLineAsync()).ReturnsAsync(null as string);

            var channelReader = StreamChannelHelper.ToChannelReader((mock.Object, StreamSource.Output));

            var results = await channelReader.ReadAllAsync(CancellationToken.None).ToListAsync();

            results.Should().BeEmpty();

            mock.VerifyAll();
        }

        [Fact]
        public async Task TestReadingTextFromStream()
        {
            TextReader textReader = new StringReader("Hello\r\nWorld");

            var channelReader = StreamChannelHelper.ToChannelReader((textReader, StreamReaderSource.StreamReaderOne));

            var allText = await channelReader.ReadAllAsync(CancellationToken.None).Select(x => x.text).ToListAsync();

            allText.Should().BeEquivalentTo("Hello","World");
        }


        [Fact]
        public async Task TestReadingTwoStreams()
        {
            TextReader textReader1 = new StringReader("Hello\r\nWorld");
            TextReader textReader2 = new StringReader("Goodbye\r\nEarth");

            var channelReader = StreamChannelHelper.ToChannelReader(
                (textReader1, StreamReaderSource.StreamReaderOne),
                (textReader2, StreamReaderSource.StreamReaderTwo)

                );

            var allText = await channelReader.ReadAllAsync(CancellationToken.None).ToListAsync();

            allText.Should().BeEquivalentTo(
                ("Hello", StreamReaderSource.StreamReaderOne),
                ("World", StreamReaderSource.StreamReaderOne),
                ("Goodbye", StreamReaderSource.StreamReaderTwo),
                ("Earth", StreamReaderSource.StreamReaderTwo));
        }

        [Fact]
        public async Task TestWritingToStream()
        {
            var stringBuilder = new StringBuilder();

            var textWriter = new StringWriter(stringBuilder);


            var writer = textWriter.ToChannelWriter();


            await writer.WriteAsync("Hello");

            await writer.WriteAsync("World");

            var completed = writer.TryComplete();
            completed.Should().BeTrue("Writer should have completed successfully");

            var tries = 0;
            while (tries < 20)
            {
                if (stringBuilder.Length > 8)
                    break;
                await Task.Delay(2);
                tries++;
            }

            stringBuilder.ToString().Should().Be("Hello\r\nWorld\r\n");

        }
    }
}
