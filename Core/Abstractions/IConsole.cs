using System.IO;

namespace Reductech.EDR.Core.Abstractions;

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
