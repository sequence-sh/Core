using System;
using System.IO;

namespace Reductech.EDR.Core.Abstractions
{

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

}
