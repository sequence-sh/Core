using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Reductech.Sequence.Core.Abstractions;

namespace Reductech.Sequence.Core;

/// <summary>
/// The state monad that is passed between steps.
/// Mutable.
/// </summary>
public sealed class StateMonad : IStateMonad
{
    /// <summary>
    /// The Connectors key in the settings entity
    /// </summary>
    public const string ConnectorsKey = "Connectors";

    private readonly ConcurrentDictionary<VariableName, ISCLObject> _stateDictionary = new();

    /// <summary>
    /// Create the settings entity from the Step Factory Store
    /// </summary>
    public static Entity CreateSettingsEntity(StepFactoryStore stepFactoryStore)
    {
        var connectorsSetting = ISCLObject.CreateFromCSharpObject(
            stepFactoryStore.ConnectorData
                .ToDictionary(
                    x => x.ConnectorSettings.Id,
                    x => ISCLObject.CreateFromCSharpObject(x.ConnectorSettings)
                )
        );

        var settings = Entity.Create((ConnectorsKey, connectorsSetting));

        return settings;
    }

    /// <summary>
    /// Create a new StateMonad
    /// </summary>
    public StateMonad(
        ILogger logger,
        StepFactoryStore stepFactoryStore,
        IExternalContext externalContext,
        IReadOnlyDictionary<string, object> sequenceMetadata)
    {
        Logger           = logger;
        StepFactoryStore = stepFactoryStore;
        ExternalContext  = externalContext;
        SequenceMetadata = sequenceMetadata;

        Settings = CreateSettingsEntity(stepFactoryStore);
    }

    /// <summary>
    /// The logger that steps will use to output messages.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The step factory store. Maps from step names to step factories.
    /// </summary>
    public StepFactoryStore StepFactoryStore { get; }

    /// <summary>
    /// The external context
    /// </summary>
    public IExternalContext ExternalContext { get; }

    /// <summary>
    /// Constant metadata for the entire sequence
    /// </summary>
    public IReadOnlyDictionary<string, object> SequenceMetadata { get; }

    /// <summary>
    /// Gets all VariableNames and associated objects.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<KeyValuePair<VariableName, ISCLObject>> GetState() => _stateDictionary;

    /// <summary>
    /// Gets the current value of this variable.
    /// </summary>
    public Result<T, IErrorBuilder> GetVariable<T>(VariableName key) where T : ISCLObject
    {
        if (Disposed)
            throw new ObjectDisposedException("State Monad was disposed");

        var r = TryGetVariableFromDictionary<T>(key, _stateDictionary)
            .Bind(
                x =>
                    x.ToResult<T, IErrorBuilder>(new ErrorBuilder(ErrorCode.MissingVariable, key))
            );

        return r;
    }

    /// <summary>
    /// Tries to get a variable from a dictionary.
    /// </summary>
    public static Result<Maybe<T>, IErrorBuilder> TryGetVariableFromDictionary<T>(
        VariableName key,
        IReadOnlyDictionary<VariableName, ISCLObject> dictionary) where T : ISCLObject
    {
        if (!dictionary.TryGetValue(key, out var value))
            return Maybe<T>.None;

        var result = value.TryConvertTyped<T>(key.Serialize(SerializeOptions.Serialize));

        if (result.IsSuccess)
            return Maybe<T>.From(result.Value);

        return result.ConvertFailure<Maybe<T>>();
    }

    /// <summary>
    /// Returns whether a particular variable has been set and not removed.
    /// </summary>
    public bool VariableExists(VariableName variable) => _stateDictionary.ContainsKey(variable);

    /// <summary>
    /// Creates or set the value of this variable.
    /// </summary>
    public async Task<Result<Unit, IError>> SetVariableAsync<T>(
        VariableName key,
        T variable,
        bool disposeOld,
        IStep? callingStep,
        CancellationToken cancellation) where T : ISCLObject
    {
        if (Disposed)
            throw new ObjectDisposedException("State Monad was disposed");

        await RemoveVariableAsync(key, disposeOld, callingStep);

        ISCLObject value;

        if (variable is IArray arrayVariable)
        {
            var result = await arrayVariable.EnsureEvaluated(cancellation);

            if (result.IsFailure)
                return result.ConvertFailure<Unit>();

            value = result.Value;
        }
        else
        {
            value = variable;
        }

        _stateDictionary
            .AddOrUpdate(
                key,
                _ => value,
                (_, _) => value
            );

        return Unit.Default;
    }

    /// <summary>
    /// Removes the variable if it exists.
    /// </summary>
    public async Task RemoveVariableAsync(VariableName key, bool dispose, IStep? callingStep)
    {
        if (Disposed)
            throw new ObjectDisposedException("State Monad was disposed");

        if (!_stateDictionary.Remove(key, out var variable))
            return;

        if (dispose)
            await DisposeVariableAsync(variable, this);
    }

    /// <inheritdoc />
    public Maybe<VariableName> AutomaticVariable => Maybe<VariableName>.None;

    /// <inheritdoc />
    public Entity Settings { get; }

    internal static async Task DisposeVariableAsync(object variable, IStateMonad stateMonad)
    {
        if (variable is IStateDisposable stateDisposable)
            await stateDisposable.DisposeAsync(stateMonad);

        if (variable is IDisposable disposable)
            disposable.Dispose();
    }

    /// <summary>
    /// Has this State Monad been disposed
    /// </summary>
    public bool Disposed { get; private set; }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!Disposed)
        {
            Disposed = true;

            foreach (var v in _stateDictionary.Values)
                await DisposeVariableAsync(v, this);
        }
    }
}

/// <summary>
/// An element which may require a reference to the state to dispose more effectively.
/// </summary>
public interface IStateDisposable
{
    /// <summary>
    /// Performs application defined functions associated with freeing resources
    /// </summary>
    /// <param name="state"></param>
    Task DisposeAsync(IStateMonad state);
}
