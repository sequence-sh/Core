using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal.Errors;
using Thinktecture;
using Thinktecture.Adapters;

namespace Reductech.EDR.Core.Abstractions
{

/// <summary>
/// The external context of a sequence.
/// </summary>
public sealed record ExternalContext(
    IFileSystem FileSystemHelper,
    IExternalProcessRunner ExternalProcessRunner,
    IConsole Console,
    params object[] InjectedContexts) : IExternalContext
{
    /// <summary>
    /// The real external context
    /// </summary>
    public static readonly ExternalContext Default = new(
        FileSystemAdapter.Default,
        ExternalProcesses.ExternalProcessRunner.Instance,
        new ConsoleAdapter()
    );

    /// <inheritdoc />
    public Result<T, ErrorBuilder> TryGetContext<T>()
    {
        var first = InjectedContexts.OfType<T>().TryFirst();

        if (first.HasValue)
            return first.Value;

        var error = ErrorCode.MissingContext.ToErrorBuilder(typeof(T).Name);

        return error;
    }
}

}
