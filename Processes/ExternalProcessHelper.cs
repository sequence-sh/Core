using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Processes
{
    /// <summary>
    /// Runs external processes.
    /// </summary>
    public static class ExternalProcessHelper
    {

        private enum Source
        {
            Output,
            Error
        }

        /// <summary>
        /// Runs an external process and returns the output and errors
        /// </summary>
        /// <param name="processPath">The path to the process</param>
        /// <param name="arguments">The arguments to provide to the process</param>
        /// <returns>The output of the process</returns>
        public static async IAsyncEnumerable<Result<string>> RunExternalProcess(string processPath, IEnumerable<string> arguments)
        {
            if (!File.Exists(processPath))
            {
                yield return Result.Failure<string>($"Could not find '{processPath}'");
                yield break;
            }
            
            var argumentString = string.Join(' ', arguments.Select(a => a.Contains(" ") ? $@"""{a}""": a));
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

            //Read the output one line at a time
            while (true)
            {
                var line = await multiStreamReader.ReadLineAsync();
                if (line == null) //We've reached the end of the file
                    break;
                if (line.Value.source == Source.Error)
                {
                    yield return Result.Failure<string>(line.Value.line);
                }

                yield return Result.Success(line.Value.line);
            }

            pProcess.WaitForExit();
        }
    }
}