using System;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Abstractions
{

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
