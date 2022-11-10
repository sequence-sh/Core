using System.IO;
using System.Threading.Channels;

namespace Sequence.Core.Internal;

/// <summary>
/// Converts a list of streams to a ChannelReader
/// </summary>
public static class StreamChannelHelper
{
    /// <summary>
    /// Converts a list of streams to a ChannelWriter
    /// </summary>
    public static ChannelWriter<string> ToChannelWriter(this TextWriter textWriter)
    {
        var channel = Channel.CreateUnbounded<string>();

        _ = WriteToChannel(textWriter, channel).ContinueWith(_ => textWriter.Close());

        static async Task WriteToChannel(TextWriter textWriter, ChannelReader<string> channelReader)
        {
            await foreach (var line in channelReader.ReadAllAsync())
                await textWriter.WriteLineAsync(line);

            await textWriter.FlushAsync();
            textWriter.Close();
        }

        return channel;
    }

    /// <summary>
    /// Converts a list of streams to a ChannelReader
    /// </summary>
    public static ChannelReader<(string text, T source)> ToChannelReader<T>(
        params (TextReader textReader, T source)[] streams)
    {
        var channel = Channel.CreateUnbounded<(string line, T source)>();

        var tasks = new List<Task>();

        foreach (var (textReader, source) in streams)
        {
            var task = WriteToChannel(textReader, source, channel.Writer);
            tasks.Add(task);
        }

        Task.WhenAll(tasks).ContinueWith((t) => channel.Writer.Complete(t.Exception));

        return channel.Reader;

        static async Task WriteToChannel(
            TextReader textReader,
            T streamSource,
            ChannelWriter<(string line, T source)> channel)
        {
            while (true)
            {
                var line = await textReader.ReadLineAsync();

                if (line == null)
                    return;

                await channel.WriteAsync((line, streamSource));
            }
        }
    }
}
