using Reductech.EDR.Core.ExternalProcesses;

namespace Reductech.EDR.Core.Abstractions;

/// <summary>
/// The external context of a sequence.
/// </summary>
public sealed record ExternalContext(
    IExternalProcessRunner ExternalProcessRunner,
    IRestClientFactory RestClientFactory,
    IConsole Console,
    params (string name, object context)[] InjectedContexts) : IExternalContext
{
    /// <summary>
    /// The real external context
    /// </summary>
    public static readonly ExternalContext Default = new(
        ExternalProcesses.ExternalProcessRunner.Instance,
        DefaultRestClientFactory.Instance,
        ConsoleAdapter.Instance
    );

    /// <inheritdoc />
    public Result<T, IErrorBuilder> TryGetContext<T>(string key) where T : class
    {
        var first = InjectedContexts
            .Where(x => x.name.Equals(key, StringComparison.OrdinalIgnoreCase))
            .TryFirst();

        if (first.HasValue && first.GetValueOrThrow().context is T context)
        {
            return context;
        }

        var error = ErrorCode.MissingContext.ToErrorBuilder(typeof(T).Name);
        return error;
    }
}
