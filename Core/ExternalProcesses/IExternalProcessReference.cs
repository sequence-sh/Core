using System;
using System.IO;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.ExternalProcesses
{
    /// <summary>
    /// A reference to a running external process.
    /// </summary>
    public interface IExternalProcessReference : IDisposable
    {
        /// <summary>
        /// The output stream of the process
        /// </summary>
        IStreamReader<(string line, StreamSource source)> OutputStream { get; }

        /// <summary>
        /// The input stream of the process.
        /// </summary>
        StreamWriter InputStream { get; }

        /// <summary>
        /// Wait for the process to exit
        /// </summary>
        void WaitForExit(int? milliseconds);
    }
}