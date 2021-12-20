using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Reductech.Sequence.Core.Internal.Logging;

namespace Reductech.Sequence.Core.ExternalProcesses;

/// <summary>
/// Basic external step runner.
/// </summary>
public class ExternalProcessRunner : IExternalProcessRunner
{
    private ExternalProcessRunner() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static IExternalProcessRunner Instance { get; } = new ExternalProcessRunner();

    /// <inheritdoc />
    public Result<IExternalProcessReference, IErrorBuilder> StartExternalProcess(
        string processPath,
        IEnumerable<string> arguments,
        IReadOnlyDictionary<string, string> environmentVariables,
        Encoding encoding,
        IStateMonad stateMonad,
        IStep? callingStep)
    {
        var absolutePath = GetAbsolutePath(processPath);

        if (absolutePath.IsFailure)
            return absolutePath.ConvertFailure<IExternalProcessReference>();

        var argumentString = string.Join(' ', arguments.Select(EncodeParameterArgument));

        var pProcess = new Process
        {
            StartInfo =
            {
                FileName               = absolutePath.Value,
                Arguments              = argumentString,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                WindowStyle            = ProcessWindowStyle.Hidden, //don't display a window
                CreateNoWindow         = true,
                StandardErrorEncoding  = encoding,
                StandardOutputEncoding = encoding,
                RedirectStandardInput  = true,
            }
        };

        foreach (var (key, value) in environmentVariables)
        {
            pProcess.StartInfo.EnvironmentVariables.Add(
                key,
                value
            );
        }

        LogSituation.ExternalProcessStarted.Log(
            stateMonad,
            callingStep,
            absolutePath.Value,
            argumentString
        );

        foreach (var (key, value) in pProcess.StartInfo.Environment)
        {
            LogSituation.EnvironmentVariable.Log(stateMonad, callingStep, key, value);
        }

        var started = pProcess.Start();

        if (!started)
            return new ErrorBuilder(ErrorCode.ExternalProcessError, "Could not start");

        var reference = new ExternalProcessReference(pProcess);

        AppDomain.CurrentDomain.ProcessExit  += (_, _) => reference.Dispose();
        AppDomain.CurrentDomain.DomainUnload += (_, _) => reference.Dispose();

        return reference;
    }

    /// <inheritdoc />
    public async Task<Result<Unit, IErrorBuilder>> RunExternalProcess(
        string processPath,
        IErrorHandler errorHandler,
        IEnumerable<string> arguments,
        IReadOnlyDictionary<string, string> environmentVariables,
        Encoding encoding,
        IStateMonad stateMonad,
        IStep? callingStep,
        CancellationToken cancellationToken)
    {
        var absolutePath = GetAbsolutePath(processPath);

        if (absolutePath.IsFailure)
            return absolutePath.ConvertFailure<Unit>();

        var argumentString = string.Join(' ', arguments.Select(EncodeParameterArgument));

        LogSituation.ExternalProcessStarted.Log(
            stateMonad,
            callingStep,
            absolutePath.Value,
            argumentString
        );

        using var pProcess = new Process
        {
            StartInfo =
            {
                FileName               = absolutePath.Value,
                Arguments              = argumentString,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                WindowStyle            = ProcessWindowStyle.Hidden, //don't display a window
                CreateNoWindow         = true,
                StandardErrorEncoding  = encoding,
                StandardOutputEncoding = encoding
            }
        };

        foreach (var (key, value) in environmentVariables)
        {
            pProcess.StartInfo.EnvironmentVariables.Add(
                key,
                value
            );
        }

        foreach (var (key, value) in pProcess.StartInfo.Environment)
        {
            LogSituation.EnvironmentVariable.Log(stateMonad, callingStep, key, value);
        }

        var errors = new List<IErrorBuilder>();

        try
        {
            pProcess.Start();

            var channelReader =
                StreamChannelHelper.ToChannelReader(
                    (pProcess.StandardOutput, StreamSource.Output),
                    (pProcess.StandardError, StreamSource.Error)
                );

            await foreach (var (line, streamSource) in channelReader.ReadAllAsync(cancellationToken)
                          )
            {
                if (streamSource == StreamSource.Error)
                {
                    var errorText = string.IsNullOrWhiteSpace(line) ? "Unknown Error" : line;

                    if (errorHandler.ShouldIgnoreError(errorText))
                        stateMonad.Log(LogLevel.Warning, line, callingStep);
                    else
                        errors.Add(new ErrorBuilder(ErrorCode.ExternalProcessError, errorText));
                }
                else
                    stateMonad.Log(LogLevel.Information, line, callingStep);
            }

            // ReSharper disable once MethodHasAsyncOverloadWithCancellation - run on a separate thread
            pProcess.WaitForExit();
        }
        #pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception e)
        {
            errors.Add(new ErrorBuilder(e, ErrorCode.ExternalProcessError));
        }
        #pragma warning restore CA1031 // Do not catch general exception types

        if (errors.Any())
        {
            var e = ErrorBuilderList.Combine(errors);
            return Result.Failure<Unit, IErrorBuilder>(e);
        }

        return Unit.Default;
    }

    /// <summary>
    /// Matches an escaped character or a quotation mark
    /// </summary>
    private static readonly Regex BackslashRegex = new(
        @"(\\*)" + "\"",
        RegexOptions.Compiled
    );

    private static readonly Regex TermWithSpaceRegex = new(
        @"^(.*\s.*?)(\\*)$",
        RegexOptions.Compiled | RegexOptions.Singleline
    );

    private static Result<string, IErrorBuilder> GetAbsolutePath(string processPath)
    {
        if (!File.Exists(processPath))
        {
            var environmentPath = Environment.GetEnvironmentVariable("PATH") ?? "";

            var paths = environmentPath.Split(Path.PathSeparator)
                .Distinct()
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            foreach (var path in paths)
            {
                foreach (var executableExtension in ExecutableExtensions)
                {
                    var executable = processPath + executableExtension;
                    var newPath    = Path.Combine(path, executable);

                    if (File.Exists(newPath))
                    {
                        return newPath;
                    }
                }
            }

            return new ErrorBuilder(ErrorCode.ExternalProcessNotFound, processPath);
        }

        return processPath;
    }

    private static readonly string[] ExecutableExtensions = new[] { "", ".exe" };
    // { ".exe", ".com", ".bat", ".sh", ".vbs", ".vbscript", ".vbe", ".js", ".rb", ".cmd", ".cpl", ".ws", ".wsf", ".msc", ".gadget" };

    private static string EncodeParameterArgument(string original)
    {
        if (string.IsNullOrEmpty(original))
            return $"\"{original}\"";

        var value = BackslashRegex.Replace(original, @"$1\$0");

        value = TermWithSpaceRegex.Replace(value, "\"$1$2$2\"");
        return value;
    }
}
