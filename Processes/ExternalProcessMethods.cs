using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Runs external processes.
    /// </summary>
    public interface IExternalProcessRunner
    {
        /// <summary>
        /// Runs an external process and returns the output and errors
        /// </summary>
        /// <param name="processPath">The path to the process</param>
        /// <param name="logger"></param>
        /// <param name="callingProcessName">The name of the calling process. For error reporting.</param>
        /// <param name="errorHandler">The error handler.</param>
        /// <param name="arguments">The arguments to provide to the process. These will all be escaped</param>
        /// <returns>The output of the process</returns>
        Task<Result<Unit, IRunErrors>> RunExternalProcess(
            string processPath,
            ILogger logger,
            string callingProcessName,
            IErrorHandler errorHandler,
            IEnumerable<string> arguments);
    }

    /// <summary>
    /// Determines how to handle errors coming from an external process.
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// Whether to ignore a particular error.
        /// </summary>
        public bool ShouldIgnoreError(string s);
    }

    /// <summary>
    /// Does not ignore any errors.
    /// </summary>
    public class IgnoreNoneErrorHandler : IErrorHandler
    {
        private IgnoreNoneErrorHandler(){}

        /// <summary>
        /// The instance.
        /// </summary>
        public static IErrorHandler Instance = new IgnoreNoneErrorHandler();

        /// <inheritdoc />
        public bool ShouldIgnoreError(string s) => false;
    }

    /// <summary>
    /// Ignores all errors.
    /// </summary>
    public class IgnoreAllErrorHandler : IErrorHandler
    {
        private IgnoreAllErrorHandler(){}

        /// <summary>
        /// The instance.
        /// </summary>
        public static IErrorHandler Instance = new IgnoreAllErrorHandler();

        /// <inheritdoc />
        public bool ShouldIgnoreError(string s) => true;
    }



    /// <summary>
    /// Basic external process runner.
    /// </summary>
    public class ExternalProcessRunner : IExternalProcessRunner
    {
        private ExternalProcessRunner() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static IExternalProcessRunner Instance { get; } = new ExternalProcessRunner();

        private enum Source
        {
            Output,
            Error
        }


        /// <inheritdoc />
        public async Task<Result<Unit, IRunErrors>> RunExternalProcess(string processPath, ILogger logger, string callingProcessName, IErrorHandler errorHandler, IEnumerable<string> arguments)
        {
            if (!File.Exists(processPath))
                return new RunError($"Could not find '{processPath}'", callingProcessName, null, ErrorCode.ExternalProcessNotFound);

            var argumentString = string.Join(' ', arguments.Select(EncodeParameterArgument));
            using var pProcess = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = processPath,
                    Arguments = argumentString,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden, //don't display a window
                    CreateNoWindow = true,
                    StandardErrorEncoding = System.Text.Encoding.UTF8,
                    StandardOutputEncoding = System.Text.Encoding.UTF8
                }
            };
            pProcess.Start();

            var multiStreamReader = new MultiStreamReader<(string line, Source source)>(new IStreamReader<(string, Source)>[]
            {
                new StreamReaderWithSource<Source>(pProcess.StandardOutput, Source.Output),
                new StreamReaderWithSource<Source>(pProcess.StandardError, Source.Error),
            });

            var errors = new List<RunError>();

            //Read the output one line at a time
            while (true)
            {
                var line = await multiStreamReader.ReadLineAsync();
                if (line == null) //We've reached the end of the file
                    break;
                if (line.Value.source == Source.Error)
                {
                    var errorText = string.IsNullOrWhiteSpace(line.Value.line) ? "Unknown Error" : line.Value.line;

                    if (errorHandler.ShouldIgnoreError(errorText))
                        logger.LogWarning(line.Value.line);
                    else
                        errors.Add(new RunError(errorText, callingProcessName, null, ErrorCode.ExternalProcessError));

                }
                else
                    logger.LogInformation(line.Value.line);
            }

            pProcess.WaitForExit();

            if (errors.Any())
            {
                var e = RunErrorList.Combine(errors);
                return Result.Failure<Unit, IRunErrors>(e);
            }

            return Unit.Default;
        }

        private static readonly Regex BackslashRegex = new Regex(@"(\\*)" + "\"", RegexOptions.Compiled);
        private static readonly Regex TermWithSpaceRegex = new Regex(@"^(.*\s.*?)(\\*)$", RegexOptions.Compiled | RegexOptions.Singleline);

        private static string EncodeParameterArgument(string original)
        {
            if (string.IsNullOrEmpty(original))
                return $"\"{original}\"";
            var value = BackslashRegex.Replace(original, @"$1\$0");

            value = TermWithSpaceRegex.Replace(value, "\"$1$2$2\"");
            return value;
        }
    }
}