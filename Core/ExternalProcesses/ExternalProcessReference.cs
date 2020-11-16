using System.Diagnostics;
using System.Threading.Channels;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.ExternalProcesses
{
    /// <summary>
    /// A reference to a running external process.
    /// </summary>
    public class ExternalProcessReference : IExternalProcessReference
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
                (process.StandardError, StreamSource.Error));

            InputChannel = process.StandardInput.ToChannelWriter();
        }

        public Process Process { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Process.Kill(true);
            Process.Dispose();
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
}