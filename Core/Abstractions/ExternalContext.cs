using System;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Abstractions
{

/// <summary>
/// Provides methods for interacting with the console
/// </summary>
public interface IConsole
{
    /// <summary>
    /// Writes the specified value, followed by the current line terminator to the standard output stream.
    /// </summary>
    void WriteLine(string? value);

    /// <summary>
    /// Acquires the standard input stream
    /// </summary>
    Stream OpenStandardInput();

    /// <summary>
    /// Acquires the standard output stream
    /// </summary>
    Stream OpenStandardOutput();

    /// <summary>
    /// Acquires the standard error stream
    /// </summary>
    Stream OpenStandardError();
}

/// <summary>
/// Default IConsole Adapter.
/// Uses System.Console
/// </summary>
public sealed class ConsoleAdapter : IConsole
{
    private ConsoleAdapter() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static ConsoleAdapter Instance { get; } = new();

    /// <inheritdoc />
    public void WriteLine(string? value) => Console.WriteLine(value);

    /// <inheritdoc />
    public Stream OpenStandardInput() => Console.OpenStandardInput();

    /// <inheritdoc />
    public Stream OpenStandardOutput() => Console.OpenStandardOutput();

    /// <inheritdoc />
    public Stream OpenStandardError() => Console.OpenStandardError();
}

/// <summary>
/// The external context of a sequence.
/// </summary>
public sealed record ExternalContext(
    IExternalProcessRunner ExternalProcessRunner,
    IConsole Console,
    params (string name, object context)[] InjectedContexts) : IExternalContext
{
    /// <summary>
    /// The real external context
    /// </summary>
    public static readonly ExternalContext Default = new(
        ExternalProcesses.ExternalProcessRunner.Instance,
        ConsoleAdapter.Instance
    );

    /// <inheritdoc />
    public Result<T, IErrorBuilder> TryGetContext<T>(string key) where T : class
    {
        var first = InjectedContexts
            .Where(x => x.name.Equals(key, StringComparison.OrdinalIgnoreCase))
            .TryFirst();

        if (first.HasValue && first.Value.context is T context)
        {
            return context;
        }

        var error = ErrorCode.MissingContext.ToErrorBuilder(typeof(T).Name);
        return error;
    }
}

}
