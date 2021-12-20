namespace Reductech.Sequence.Core.Internal.Errors;

/// <summary>
/// An exception that wraps an error
/// </summary>
public class ErrorException : Exception
{
    /// <summary>
    /// Create a new ErrorException
    /// </summary>
    public ErrorException(IError error) => Error = error;

    /// <summary>
    /// The errorBuilder
    /// </summary>
    public IError Error { get; }
}
