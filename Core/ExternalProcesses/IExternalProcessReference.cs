using System.Threading.Channels;

namespace Sequence.Core.ExternalProcesses;

/// <summary>
/// A reference to a running external process.
/// </summary>
public interface IExternalProcessReference : IDisposable
{
    /// <summary>
    /// The output stream of the process
    /// </summary>
    ChannelReader<(string line, StreamSource source)> OutputChannel { get; }

    /// <summary>
    /// The input stream of the process.
    /// </summary>
    ChannelWriter<string> InputChannel { get; }

    /// <summary>
    /// Wait for the process to exit
    /// </summary>
    void WaitForExit(int? milliseconds);
}
