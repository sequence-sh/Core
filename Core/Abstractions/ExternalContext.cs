using Sequence.Core.ExternalProcesses;

namespace Sequence.Core.Abstractions;

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

/// <summary>
/// An external context that cannot access the external world. Throws an exception if it tries.
/// </summary>
public sealed record NullExternalContext : IExternalContext
{
    /// <summary>
    /// The instance
    /// </summary>
    public static NullExternalContext Instance { get; } = new();

    /// <inheritdoc />
    public IExternalProcessRunner ExternalProcessRunner => throw new NotSupportedException(
        nameof(NullExternalContext) + " cannot produce " + nameof(ExternalProcessRunner)
    );

    /// <inheritdoc />
    public IRestClientFactory RestClientFactory => throw new NotSupportedException(
        nameof(NullExternalContext) + " cannot produce " + nameof(RestClientFactory)
    );

    /// <inheritdoc />
    public IConsole Console => throw new NotSupportedException(
        nameof(NullExternalContext) + " cannot produce " + nameof(Console)
    );

    /// <inheritdoc />
    public Result<T, IErrorBuilder> TryGetContext<T>(string name) where T : class
    {
        var error = ErrorCode.MissingContext.ToErrorBuilder(typeof(T).Name);
        return error;
    }

    /// <inheritdoc />
    public (string name, object context)[] InjectedContexts =>
        Array.Empty<(string name, object context)>();
}
