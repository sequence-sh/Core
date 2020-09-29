using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core
{
    /// <summary>
    /// Runs external processes.
    /// </summary>
    public interface IExternalProcessRunner
    {
        /// <summary>
        /// Runs an external step and returns the output and errors
        /// </summary>
        /// <param name="processPath">The path to the step</param>
        /// <param name="logger"></param>
        /// <param name="callingProcessName">The name of the calling step. For error reporting.</param>
        /// <param name="errorHandler">The error handler.</param>
        /// <param name="arguments">The arguments to provide to the step. These will all be escaped</param>
        /// <returns>The output of the step</returns>
        Task<Result<Unit, IRunErrors>> RunExternalProcess(
            string processPath,
            ILogger logger,
            string callingProcessName,
            IErrorHandler errorHandler,
            IEnumerable<string> arguments);
    }
}