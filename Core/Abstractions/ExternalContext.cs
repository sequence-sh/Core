using System;
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
    params (string name, object context)[] InjectedContexts) : IExternalContext
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
    public Result<T, ErrorBuilder> TryGetContext<T>(string key) where T : class
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
