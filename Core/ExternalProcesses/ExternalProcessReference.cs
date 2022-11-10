using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Channels;

namespace Sequence.Core.ExternalProcesses;

/// <summary>
/// A reference to a running external process.
/// </summary>
public sealed class ExternalProcessReference : IExternalProcessReference
{
    /// <summary>
    /// Create a new ExternalProcessReference
    /// </summary>
    /// <param name="process"></param>
    public ExternalProcessReference(Process process)
    {
        Process = process;

        OutputChannel = StreamChannelHelper.ToChannelReader(
            (process.StandardOutput, StreamSource.Output),
            (process.StandardError, StreamSource.Error)
        );

        InputChannel = process.StandardInput.ToChannelWriter();
    }

    /// <summary>
    /// The external process.
    /// </summary>
    public Process Process { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        try
        {
            if (!Process.HasExited)
                Process.Kill(true);

            Process.Dispose();
        }
        catch (Win32Exception) { }
        catch (InvalidOperationException) { }
    }

    /// <inheritdoc />
    public ChannelReader<(string line, StreamSource source)> OutputChannel { get; }

    /// <inheritdoc />
    public ChannelWriter<string> InputChannel { get; }

    /// <inheritdoc />
    public void WaitForExit(int? milliseconds)
    {
        if (milliseconds.HasValue)
            Process.WaitForExit(milliseconds.Value);
        else
            Process.WaitForExit();
    }
}
