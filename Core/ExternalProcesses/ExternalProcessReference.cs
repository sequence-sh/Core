using System.Diagnostics;
using System.IO;
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
            OutputStream = new MultiStreamReader<(string line, StreamSource source)>(new IStreamReader<(string, StreamSource)>[]
            {
                new StreamReaderWithSource<StreamSource>(process.StandardOutput, StreamSource.Output),
                new StreamReaderWithSource<StreamSource>(process.StandardError, StreamSource.Error),
            });
            InputStream = process.StandardInput;
        }

        public Process Process { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Process.Kill(true);
            Process.Dispose();
        }

        /// <inheritdoc />
        public IStreamReader<(string line, StreamSource source)> OutputStream { get; }

        /// <inheritdoc />
        public StreamWriter InputStream { get; }

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